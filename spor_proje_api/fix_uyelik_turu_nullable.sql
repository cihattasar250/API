-- UyelikTuru kolonunu nullable yap
-- Migration: MakeUyelikTuruNullable

ALTER TABLE [Uyeler]
ALTER COLUMN [UyelikTuru] INT NULL;

