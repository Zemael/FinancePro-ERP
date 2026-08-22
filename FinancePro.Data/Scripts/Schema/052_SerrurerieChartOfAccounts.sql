IF OBJECT_ID(N'dbo.Empresas', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.PlanoContas', N'U') IS NOT NULL
BEGIN
    DECLARE @Plano TABLE (Codigo nvarchar(20), Nome nvarchar(150), Tipo nvarchar(30), CodigoPai nvarchar(20) NULL);
    INSERT INTO @Plano (Codigo, Nome, Tipo, CodigoPai) VALUES
    (N'1', N'ATIVO', N'Ativo', NULL),
    (N'1.1', N'Caixa', N'Ativo', N'1'),
    (N'1.2', N'Bancos', N'Ativo', N'1'),
    (N'1.3', N'Clientes a receber', N'Ativo', N'1'),
    (N'1.4', N'Estoque de matéria-prima (chapas, perfis, tubos)', N'Ativo', N'1'),
    (N'1.5', N'Estoque de produtos acabados', N'Ativo', N'1'),
    (N'1.6', N'Máquinas e equipamentos', N'Ativo', N'1'),
    (N'1.7', N'Veículos', N'Ativo', N'1'),
    (N'1.8', N'Ferramentas', N'Ativo', N'1'),
    (N'1.9', N'Imóveis/instalações', N'Ativo', N'1'),
    (N'2', N'PASSIVO', N'Passivo', NULL),
    (N'2.1', N'Fornecedores a pagar', N'Passivo', N'2'),
    (N'2.2', N'Empréstimos e financiamentos', N'Passivo', N'2'),
    (N'2.3', N'Salários a pagar', N'Passivo', N'2'),
    (N'2.4', N'Impostos a pagar', N'Passivo', N'2'),
    (N'3', N'PATRIMÓNIO LÍQUIDO', N'PatrimonioLiquido', NULL),
    (N'3.1', N'Capital social', N'PatrimonioLiquido', N'3'),
    (N'3.2', N'Lucros/prejuízos acumulados', N'PatrimonioLiquido', N'3'),
    (N'4', N'RECEITAS', N'Receita', NULL),
    (N'4.1', N'Venda de serviços de serralharia', N'Receita', N'4'),
    (N'4.2', N'Venda de produtos', N'Receita', N'4'),
    (N'4.3', N'Instalação em obra', N'Receita', N'4'),
    (N'4.4', N'Outras receitas', N'Receita', N'4'),
    (N'5', N'CUSTOS/DESPESAS', N'Despesa', NULL),
    (N'5.1', N'Matéria-prima consumida', N'Despesa', N'5'),
    (N'5.2', N'Mão de obra direta', N'Despesa', N'5'),
    (N'5.3', N'Energia elétrica', N'Despesa', N'5'),
    (N'5.4', N'Manutenção de máquinas', N'Despesa', N'5'),
    (N'5.5', N'Aluguel', N'Despesa', N'5'),
    (N'5.6', N'Salários administrativos', N'Despesa', N'5'),
    (N'5.7', N'Marketing/comercial', N'Despesa', N'5'),
    (N'5.8', N'Impostos e taxas', N'Despesa', N'5'),
    (N'5.9', N'Depreciação', N'Despesa', N'5');

    INSERT INTO dbo.PlanoContas (Codigo, Nome, Tipo, ContaPaiId, EmpresaId, Ativo, DataCriacao)
    SELECT p.Codigo, p.Nome, p.Tipo, NULL, e.Id, 1, SYSUTCDATETIME()
    FROM dbo.Empresas e CROSS JOIN @Plano p
    WHERE p.CodigoPai IS NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.PlanoContas x WHERE x.EmpresaId=e.Id AND x.Codigo=p.Codigo);

    INSERT INTO dbo.PlanoContas (Codigo, Nome, Tipo, ContaPaiId, EmpresaId, Ativo, DataCriacao)
    SELECT p.Codigo, p.Nome, p.Tipo, pai.Id, e.Id, 1, SYSUTCDATETIME()
    FROM dbo.Empresas e
    INNER JOIN dbo.PlanoContas pai ON pai.EmpresaId=e.Id
    CROSS JOIN @Plano p
    WHERE p.CodigoPai IS NOT NULL AND pai.Codigo=p.CodigoPai
      AND NOT EXISTS (SELECT 1 FROM dbo.PlanoContas x WHERE x.EmpresaId=e.Id AND x.Codigo=p.Codigo);

    IF OBJECT_ID(N'dbo.EnterpriseChartAccounts', N'U') IS NOT NULL
    BEGIN
        INSERT INTO dbo.EnterpriseChartAccounts
            (CompanyId, Code, Name, ParentId, AccountType, Nature, AllowsPosting, RequiresCostCenter, Active, CreatedAt, UpdatedAt)
        SELECT e.Id, p.Codigo, p.Nome, NULL, N'Sintética',
               CASE WHEN p.Tipo IN (N'Ativo', N'Despesa') THEN N'Devedora' ELSE N'Credora' END,
               0, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
        FROM dbo.Empresas e CROSS JOIN @Plano p
        WHERE p.CodigoPai IS NULL
          AND NOT EXISTS (SELECT 1 FROM dbo.EnterpriseChartAccounts x WHERE x.CompanyId=e.Id AND x.Code=p.Codigo);

        INSERT INTO dbo.EnterpriseChartAccounts
            (CompanyId, Code, Name, ParentId, AccountType, Nature, AllowsPosting, RequiresCostCenter, Active, CreatedAt, UpdatedAt)
        SELECT e.Id, p.Codigo, p.Nome, pai.Id, N'Analítica',
               CASE WHEN p.Tipo IN (N'Ativo', N'Despesa') THEN N'Devedora' ELSE N'Credora' END,
               1, CASE WHEN p.Tipo=N'Despesa' THEN 1 ELSE 0 END, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
        FROM dbo.Empresas e
        INNER JOIN dbo.EnterpriseChartAccounts pai ON pai.CompanyId=e.Id
        CROSS JOIN @Plano p
        WHERE p.CodigoPai IS NOT NULL AND pai.Code=p.CodigoPai
          AND NOT EXISTS (SELECT 1 FROM dbo.EnterpriseChartAccounts x WHERE x.CompanyId=e.Id AND x.Code=p.Codigo);
    END
END
