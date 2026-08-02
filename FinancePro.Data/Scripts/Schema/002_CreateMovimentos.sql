-- FinancePro ERP — Etapa 5 (Tesouraria): tabela de lançamentos
-- Toda receita/despesa fica associada a exatamente uma origem
-- (CaixaId XOR ContaBancariaId), garantido pelo CHECK abaixo.

CREATE TABLE Movimentos (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Data            DATE            NOT NULL,
    Descricao       NVARCHAR(200)   NOT NULL,
    Valor           DECIMAL(18,2)   NOT NULL,
    Tipo            NVARCHAR(20)    NOT NULL, -- Receita | Despesa
    CategoriaId     INT             NULL,
    CaixaId         INT             NULL,
    ContaBancariaId INT             NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_Movimentos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Movimentos_Caixas FOREIGN KEY (CaixaId) REFERENCES Caixas(Id),
    CONSTRAINT FK_Movimentos_ContasBancarias FOREIGN KEY (ContaBancariaId) REFERENCES ContasBancarias(Id),
    CONSTRAINT FK_Movimentos_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id),
    CONSTRAINT CK_Movimentos_UmaOrigem CHECK (
        (CaixaId IS NOT NULL AND ContaBancariaId IS NULL) OR
        (CaixaId IS NULL AND ContaBancariaId IS NOT NULL)
    )
);

CREATE INDEX IX_Movimentos_Empresa_Data ON Movimentos (EmpresaId, Data);
