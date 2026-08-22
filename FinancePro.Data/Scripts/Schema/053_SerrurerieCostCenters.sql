IF OBJECT_ID(N'dbo.Empresas', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.CostCenters', N'U') IS NOT NULL
BEGIN
    DECLARE @Centros TABLE (Codigo nvarchar(30), Nome nvarchar(160));
    INSERT INTO @Centros (Codigo, Nome) VALUES
    (N'CC01', N'Administração geral'),
    (N'CC02', N'Financeiro e contabilidade'),
    (N'CC03', N'Recursos humanos'),
    (N'CC04', N'Compras e almoxarifado'),
    (N'CC05', N'Corte e conformação'),
    (N'CC06', N'Solda e montagem'),
    (N'CC07', N'Pintura e acabamento'),
    (N'CC08', N'Manutenção de máquinas'),
    (N'CC09', N'Projeto e desenho técnico'),
    (N'CC10', N'Vendas e orçamentos'),
    (N'CC11', N'Marketing'),
    (N'CC12', N'Expedição e entrega'),
    (N'CC13', N'Instalação em obra'),
    (N'CC14', N'Segurança do trabalho');

    INSERT INTO dbo.CostCenters (CompanyId, Code, Name, Active, CreatedAt, UpdatedAt)
    SELECT e.Id, c.Codigo, c.Nome, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
    FROM dbo.Empresas e CROSS JOIN @Centros c
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.CostCenters x
        WHERE x.CompanyId=e.Id AND x.Code=c.Codigo
    );
END
