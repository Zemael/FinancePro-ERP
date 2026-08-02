-- FinancePro ERP — Gestão Orçamental (Orcamento / OrcamentoDetalhe / RevisaoOrcamental)

CREATE TABLE Orcamentos (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Ano             INT             NOT NULL,
    Nome            NVARCHAR(150)   NOT NULL,
    DataInicio      DATE            NOT NULL,
    DataFim         DATE            NOT NULL,
    Moeda           NVARCHAR(20)    NOT NULL DEFAULT 'FCFA',
    Estado          NVARCHAR(20)    NOT NULL DEFAULT 'Rascunho', -- Rascunho|EmAprovacao|Aprovado|Rejeitado|Encerrado
    Observacoes     NVARCHAR(MAX)   NULL,
    ElaboradoPor    NVARCHAR(150)   NULL,
    RevistoPor      NVARCHAR(150)   NULL,
    AprovadoPor     NVARCHAR(150)   NULL,
    DataAprovacao   DATE            NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_Orcamentos_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);
CREATE INDEX IX_Orcamentos_Empresa_Ano ON Orcamentos (EmpresaId, Ano);

-- Valor Realizado NÃO tem coluna própria — é calculado em tempo real a
-- partir dos Movimentos ligados à mesma conta do Plano de Contas.
CREATE TABLE OrcamentoDetalhes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrcamentoId     INT             NOT NULL,
    PlanoContasId   INT             NOT NULL,
    CentroCusto     NVARCHAR(100)   NULL,
    Departamento    NVARCHAR(100)   NULL,
    Tipo            NVARCHAR(20)    NOT NULL, -- Receita | Despesa
    Mes             INT             NOT NULL, -- 1-12
    ValorPrevisto   DECIMAL(18,2)   NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_OrcamentoDetalhes_Orcamentos FOREIGN KEY (OrcamentoId) REFERENCES Orcamentos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrcamentoDetalhes_PlanoContas FOREIGN KEY (PlanoContasId) REFERENCES PlanoContas(Id)
);
CREATE INDEX IX_OrcamentoDetalhes_Orcamento_Mes ON OrcamentoDetalhes (OrcamentoId, Mes, Tipo);

CREATE TABLE RevisoesOrcamentais (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrcamentoId     INT             NOT NULL,
    Versao          INT             NOT NULL,
    Data            DATE            NOT NULL,
    Motivo          NVARCHAR(300)   NOT NULL,
    Responsavel     NVARCHAR(150)   NOT NULL,
    Estado          NVARCHAR(20)    NOT NULL DEFAULT 'Pendente', -- Pendente | Aprovada | Rejeitada
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_RevisoesOrcamentais_Orcamentos FOREIGN KEY (OrcamentoId) REFERENCES Orcamentos(Id) ON DELETE CASCADE
);
