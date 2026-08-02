-- FinancePro ERP — Módulo 07: Compras

CREATE TABLE Compras (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    NumeroPedido    NVARCHAR(20)    NOT NULL,
    Data            DATE            NOT NULL,
    Departamento    NVARCHAR(100)   NULL,
    CentroCusto     NVARCHAR(100)   NULL,
    Projeto         NVARCHAR(100)   NULL,
    Comprador       NVARCHAR(150)   NULL,
    Prioridade      NVARCHAR(20)    NOT NULL DEFAULT 'Normal', -- Baixa|Normal|Alta|Urgente
    Estado          NVARCHAR(20)    NOT NULL DEFAULT 'Pendente', -- Pendente|Aprovado|Rejeitado|Concluido|Cancelado
    ValorTotal      DECIMAL(18,2)   NOT NULL DEFAULT 0,
    FornecedorId    INT             NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_Compras_Fornecedores FOREIGN KEY (FornecedorId) REFERENCES Fornecedores(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Compras_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);
CREATE INDEX IX_Compras_Empresa_Data ON Compras (EmpresaId, Data);
