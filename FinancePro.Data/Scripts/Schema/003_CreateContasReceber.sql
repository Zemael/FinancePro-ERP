-- FinancePro ERP — Etapa 5 (Receitas): contas a receber

CREATE TABLE ContasReceber (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Descricao       NVARCHAR(200)   NOT NULL,
    Valor           DECIMAL(18,2)   NOT NULL,
    DataEmissao     DATE            NOT NULL,
    DataVencimento  DATE            NOT NULL,
    Estado          NVARCHAR(20)    NOT NULL DEFAULT 'Pendente', -- Pendente | Recebido | Cancelado
    DataRecebimento DATE            NULL,
    ClienteId       INT             NULL,
    CategoriaId     INT             NULL,
    MovimentoId     INT             NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_ContasReceber_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id) ON DELETE SET NULL,
    CONSTRAINT FK_ContasReceber_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE SET NULL,
    CONSTRAINT FK_ContasReceber_Movimentos FOREIGN KEY (MovimentoId) REFERENCES Movimentos(Id),
    CONSTRAINT FK_ContasReceber_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);

CREATE INDEX IX_ContasReceber_Empresa_Vencimento ON ContasReceber (EmpresaId, DataVencimento);
