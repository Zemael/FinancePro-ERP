IF OBJECT_ID('dbo.Compras','U') IS NOT NULL
BEGIN
 IF COL_LENGTH('dbo.Compras','ProjectId') IS NULL ALTER TABLE dbo.Compras ADD ProjectId int NULL;
 IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Compras') AND name='IX_Compras_ProjectId') CREATE INDEX IX_Compras_ProjectId ON dbo.Compras(ProjectId);
END;
IF OBJECT_ID('dbo.ContasReceber','U') IS NOT NULL
BEGIN
 IF COL_LENGTH('dbo.ContasReceber','ProjectId') IS NULL ALTER TABLE dbo.ContasReceber ADD ProjectId int NULL;
 IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.ContasReceber') AND name='IX_ContasReceber_ProjectId') CREATE INDEX IX_ContasReceber_ProjectId ON dbo.ContasReceber(ProjectId);
END;
IF OBJECT_ID('dbo.ContasPagar','U') IS NOT NULL
BEGIN
 IF COL_LENGTH('dbo.ContasPagar','ProjectId') IS NULL ALTER TABLE dbo.ContasPagar ADD ProjectId int NULL;
 IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.ContasPagar') AND name='IX_ContasPagar_ProjectId') CREATE INDEX IX_ContasPagar_ProjectId ON dbo.ContasPagar(ProjectId);
END;
IF OBJECT_ID('dbo.MovimentosStock','U') IS NOT NULL
BEGIN
 IF COL_LENGTH('dbo.MovimentosStock','ProjectId') IS NULL ALTER TABLE dbo.MovimentosStock ADD ProjectId int NULL;
 IF COL_LENGTH('dbo.MovimentosStock','WorkOrderId') IS NULL ALTER TABLE dbo.MovimentosStock ADD WorkOrderId int NULL;
 IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.MovimentosStock') AND name='IX_MovimentosStock_ProjectId') CREATE INDEX IX_MovimentosStock_ProjectId ON dbo.MovimentosStock(ProjectId);
END;
