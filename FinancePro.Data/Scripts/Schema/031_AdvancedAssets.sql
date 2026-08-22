SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.Bens','ValorResidual') IS NULL
    EXEC('ALTER TABLE dbo.Bens ADD ValorResidual decimal(18,2) NOT NULL CONSTRAINT DF_Bens_ValorResidual DEFAULT(0);');
IF OBJECT_ID('dbo.AssetMaintenance','U') IS NULL
BEGIN
 CREATE TABLE dbo.AssetMaintenance(
  Id int IDENTITY(1,1) PRIMARY KEY, BemId int NOT NULL, Data date NOT NULL, Tipo nvarchar(50) NOT NULL,
  Descricao nvarchar(300) NOT NULL, Valor decimal(18,2) NOT NULL CONSTRAINT DF_AssetMaintenance_Valor DEFAULT(0),
  ProximaData date NULL, CreatedAt datetime2 NOT NULL CONSTRAINT DF_AssetMaintenance_Created DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_AssetMaintenance_Bem FOREIGN KEY(BemId) REFERENCES dbo.Bens(Id));
END;
COMMIT TRANSACTION;
