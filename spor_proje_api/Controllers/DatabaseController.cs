using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spor_proje_api.Data;
using System.Data;

namespace spor_proje_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly SporDbContext _context;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(SporDbContext context, ILogger<DatabaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Üyeler tablosundan gereksiz kolonları temizler
        /// </summary>
        [HttpPost("clean-uye-columns")]
        public async Task<ActionResult> CleanUyeColumns()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                var columnsToRemove = new[]
                {
                    "AcilDurumIletisim",
                    "UyelikTuru",
                    "UyelikUcreti",
                    "PaketBaslangicTarihi",
                    "PaketBitisTarihi"
                };

                var removedColumns = new List<string>();
                var errors = new List<string>();

                // Veritabanı tipini kontrol et (SQL Server veya MySQL)
                var dbProvider = _context.Database.ProviderName;
                var isMySql = dbProvider?.Contains("MySql", StringComparison.OrdinalIgnoreCase) ?? false;
                var databaseName = connection.Database;

                // Önce UyelikUcreti kolonuna bağlı constraint'leri kaldır (SQL Server için)
                if (!isMySql)
                {
                    try
                    {
                        // SQL Server için constraint kontrolü ve kaldırma
                        var checkConstraintSql = @"
                            SELECT name 
                            FROM sys.default_constraints 
                            WHERE parent_object_id = OBJECT_ID('Uyeler')
                            AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('Uyeler'), 'UyelikUcreti', 'ColumnId')";
                        
                        using (var checkCmd = connection.CreateCommand())
                        {
                            checkCmd.CommandText = checkConstraintSql;
                            using (var reader = await checkCmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var constraintName = reader.GetString(0);
                                    var dropConstraintSql = $"ALTER TABLE Uyeler DROP CONSTRAINT [{constraintName}]";
                                    using (var dropCmd = connection.CreateCommand())
                                    {
                                        dropCmd.CommandText = dropConstraintSql;
                                        await dropCmd.ExecuteNonQueryAsync();
                                        _logger.LogInformation($"Constraint kaldırıldı: {constraintName}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Constraint kaldırılırken hata: {ex.Message}");
                    }
                }

                foreach (var columnName in columnsToRemove)
                {
                    try
                    {
                        // Önce kolonun var olup olmadığını kontrol et
                        string checkColumnSql;
                        string tableSchema = "dbo"; // SQL Server için varsayılan

                        if (isMySql)
                        {
                            // MySQL için
                            checkColumnSql = $@"
                                SELECT COUNT(*) 
                                FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_SCHEMA = '{databaseName}'
                                AND TABLE_NAME = 'Uyeler' 
                                AND COLUMN_NAME = @columnName";
                        }
                        else
                        {
                            // SQL Server için
                            checkColumnSql = @"
                                SELECT COUNT(*) 
                                FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_SCHEMA = 'dbo'
                                AND TABLE_NAME = 'Uyeler' 
                                AND COLUMN_NAME = @columnName";
                        }

                        using (var checkCmd = connection.CreateCommand())
                        {
                            checkCmd.CommandText = checkColumnSql;
                            var param = checkCmd.CreateParameter();
                            param.ParameterName = "@columnName";
                            param.Value = columnName;
                            checkCmd.Parameters.Add(param);

                            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                            if (exists > 0)
                            {
                                // Kolon varsa kaldır - MySQL ve SQL Server için aynı syntax
                                var dropColumnSql = $"ALTER TABLE Uyeler DROP COLUMN `{columnName}`";
                                if (!isMySql)
                                {
                                    // SQL Server için köşeli parantez kullan
                                    dropColumnSql = $"ALTER TABLE Uyeler DROP COLUMN [{columnName}]";
                                }

                                using (var dropCmd = connection.CreateCommand())
                                {
                                    dropCmd.CommandText = dropColumnSql;
                                    await dropCmd.ExecuteNonQueryAsync();
                                    removedColumns.Add(columnName);
                                    _logger.LogInformation($"Kolon kaldırıldı: {columnName}");
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"Kolon zaten yok: {columnName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"{columnName}: {ex.Message}";
                        errors.Add(errorMsg);
                        _logger.LogWarning($"Kolon kaldırılırken hata: {errorMsg}");
                    }
                }

                await connection.CloseAsync();

                return Ok(new
                {
                    success = true,
                    message = "Kolon temizleme işlemi tamamlandı",
                    removedColumns = removedColumns,
                    errors = errors,
                    totalRemoved = removedColumns.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kolon temizleme sırasında hata oluştu");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Kolon temizleme sırasında hata oluştu",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Üyeler tablosundaki kolonları listeler
        /// </summary>
        [HttpGet("uye-columns")]
        public async Task<ActionResult> GetUyeColumns()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                // Veritabanı tipini kontrol et
                var dbProvider = _context.Database.ProviderName;
                var isMySql = dbProvider?.Contains("MySql", StringComparison.OrdinalIgnoreCase) ?? false;
                var databaseName = connection.Database;

                string sql;
                if (isMySql)
                {
                    sql = $@"
                        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = '{databaseName}'
                        AND TABLE_NAME = 'Uyeler'
                        ORDER BY ORDINAL_POSITION";
                }
                else
                {
                    sql = @"
                        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'dbo'
                        AND TABLE_NAME = 'Uyeler'
                        ORDER BY ORDINAL_POSITION";
                }

                var columns = new List<object>();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columns.Add(new
                            {
                                ColumnName = reader.GetString(0),
                                DataType = reader.GetString(1),
                                IsNullable = reader.GetString(2),
                                MaxLength = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
                            });
                        }
                    }
                }

                await connection.CloseAsync();

                return Ok(new
                {
                    success = true,
                    columns = columns,
                    totalColumns = columns.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kolon listesi alınırken hata oluştu");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Kolon listesi alınırken hata oluştu",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Paket bilgileri kolonlarını Uyeler tablosuna ekler
        /// </summary>
        [HttpPost("add-paket-columns")]
        public async Task<ActionResult> AddPaketColumns()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                var addedColumns = new List<string>();
                var errors = new List<string>();

                // Veritabanı tipini kontrol et
                var dbProvider = _context.Database.ProviderName;
                var isMySql = dbProvider?.Contains("MySql", StringComparison.OrdinalIgnoreCase) ?? false;
                var databaseName = connection.Database;

                // Eklenecek kolonlar
                var columnsToAdd = new[]
                {
                    new { Name = "UyelikTuru", Type = "INT", Nullable = "NULL" },
                    new { Name = "PaketBaslangicTarihi", Type = "DATETIME2", Nullable = "NULL" },
                    new { Name = "PaketBitisTarihi", Type = "DATETIME2", Nullable = "NULL" }
                };

                foreach (var column in columnsToAdd)
                {
                    try
                    {
                        // Kolonun var olup olmadığını kontrol et
                        string checkColumnSql;
                        if (isMySql)
                        {
                            checkColumnSql = $@"
                                SELECT COUNT(*) 
                                FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_SCHEMA = '{databaseName}'
                                AND TABLE_NAME = 'Uyeler' 
                                AND COLUMN_NAME = @columnName";
                        }
                        else
                        {
                            checkColumnSql = @"
                                SELECT COUNT(*) 
                                FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_SCHEMA = 'dbo'
                                AND TABLE_NAME = 'Uyeler' 
                                AND COLUMN_NAME = @columnName";
                        }

                        using (var checkCmd = connection.CreateCommand())
                        {
                            checkCmd.CommandText = checkColumnSql;
                            var param = checkCmd.CreateParameter();
                            param.ParameterName = "@columnName";
                            param.Value = column.Name;
                            checkCmd.Parameters.Add(param);

                            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                            if (exists == 0)
                            {
                                // Kolon yoksa ekle
                                string addColumnSql;
                                if (isMySql)
                                {
                                    addColumnSql = $"ALTER TABLE Uyeler ADD COLUMN `{column.Name}` {column.Type} {column.Nullable}";
                                }
                                else
                                {
                                    addColumnSql = $"ALTER TABLE [Uyeler] ADD [{column.Name}] {column.Type} {column.Nullable}";
                                }

                                using (var addCmd = connection.CreateCommand())
                                {
                                    addCmd.CommandText = addColumnSql;
                                    await addCmd.ExecuteNonQueryAsync();
                                    addedColumns.Add(column.Name);
                                    _logger.LogInformation($"Kolon eklendi: {column.Name}");
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"Kolon zaten mevcut: {column.Name}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"{column.Name}: {ex.Message}";
                        errors.Add(errorMsg);
                        _logger.LogWarning($"Kolon eklenirken hata: {errorMsg}");
                    }
                }

                await connection.CloseAsync();

                return Ok(new
                {
                    success = true,
                    message = "Paket kolonları kontrol edildi ve eklendi",
                    addedColumns = addedColumns,
                    errors = errors,
                    totalAdded = addedColumns.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paket kolonları eklenirken hata oluştu");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Paket kolonları eklenirken hata oluştu",
                    error = ex.Message
                });
            }
        }
    }
}

