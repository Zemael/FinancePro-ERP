IF COL_LENGTH('dbo.ContasReceber','ComercialEstado') IS NULL
    EXEC('ALTER TABLE dbo.ContasReceber ADD ComercialEstado nvarchar(20) NULL;');

IF COL_LENGTH('dbo.ContasReceber','NumeroProposta') IS NULL
    EXEC('ALTER TABLE dbo.ContasReceber ADD NumeroProposta nvarchar(50) NULL;');

IF COL_LENGTH('dbo.ContasReceber','NumeroFatura') IS NULL
    EXEC('ALTER TABLE dbo.ContasReceber ADD NumeroFatura nvarchar(50) NULL;');

IF COL_LENGTH('dbo.ContasReceber','DataAprovacao') IS NULL
    EXEC('ALTER TABLE dbo.ContasReceber ADD DataAprovacao datetime2 NULL;');

IF COL_LENGTH('dbo.ContasReceber','DataFaturacao') IS NULL
    EXEC('ALTER TABLE dbo.ContasReceber ADD DataFaturacao datetime2 NULL;');

EXEC('
UPDATE dbo.ContasReceber
SET
    ComercialEstado = ''Faturada'',
    DataFaturacao = COALESCE(DataFaturacao, DataEmissao)
WHERE ComercialEstado IS NULL;
');

EXEC('
ALTER TABLE dbo.ContasReceber
ALTER COLUMN ComercialEstado nvarchar(20) NOT NULL;
');
