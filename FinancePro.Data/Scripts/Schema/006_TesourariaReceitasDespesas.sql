-- FinancePro ERP — Tesouraria real (MovimentoFinanceiro), Receitas e Despesas completas

ALTER TABLE Movimentos ADD TipoOperacao NVARCHAR(20) NOT NULL DEFAULT 'Entrada';
ALTER TABLE Movimentos ADD Estado NVARCHAR(20) NOT NULL DEFAULT 'Confirmado';
ALTER TABLE Movimentos ADD Conciliado BIT NOT NULL DEFAULT 0;
ALTER TABLE Movimentos ADD FormaPagamento NVARCHAR(50) NULL;
ALTER TABLE Movimentos ADD CentroCusto NVARCHAR(100) NULL;
ALTER TABLE Movimentos ADD GrupoTransferenciaId UNIQUEIDENTIFIER NULL;

ALTER TABLE Caixas ADD PermiteSaldoNegativo BIT NOT NULL DEFAULT 0;

ALTER TABLE ContasReceber ADD Codigo NVARCHAR(20) NULL;
ALTER TABLE ContasReceber ADD FormaPagamento NVARCHAR(50) NULL;
ALTER TABLE ContasReceber ADD CentroCusto NVARCHAR(100) NULL;

CREATE TABLE ContasPagar (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Codigo          NVARCHAR(20)    NULL,
    Descricao       NVARCHAR(200)   NOT NULL,
    Valor           DECIMAL(18,2)   NOT NULL,
    DataEmissao     DATE            NOT NULL,
    DataVencimento  DATE            NOT NULL,
    Estado          NVARCHAR(20)    NOT NULL DEFAULT 'Pendente', -- Pendente | Recebido(Paga) | Cancelado
    DataPagamento   DATE            NULL,
    FormaPagamento  NVARCHAR(50)    NULL,
    CentroCusto     NVARCHAR(100)   NULL,
    FornecedorId    INT             NULL,
    CategoriaId     INT             NULL,
    MovimentoId     INT             NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_ContasPagar_Fornecedores FOREIGN KEY (FornecedorId) REFERENCES Fornecedores(Id) ON DELETE SET NULL,
    CONSTRAINT FK_ContasPagar_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE SET NULL,
    CONSTRAINT FK_ContasPagar_Movimentos FOREIGN KEY (MovimentoId) REFERENCES Movimentos(Id),
    CONSTRAINT FK_ContasPagar_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);
CREATE INDEX IX_ContasPagar_Empresa_Vencimento ON ContasPagar (EmpresaId, DataVencimento);
