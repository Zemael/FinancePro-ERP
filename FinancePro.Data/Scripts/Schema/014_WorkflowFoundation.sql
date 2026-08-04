IF OBJECT_ID('dbo.WorkflowTasks','U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkflowTasks
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowTasks PRIMARY KEY,
        CompanyId INT NOT NULL,
        Module NVARCHAR(80) NOT NULL,
        DocumentType NVARCHAR(80) NOT NULL,
        DocumentReference NVARCHAR(100) NOT NULL,
        Title NVARCHAR(250) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Amount DECIMAL(18,2) NULL,
        AssignedUserId INT NOT NULL,
        AssignedUserName NVARCHAR(200) NOT NULL,
        RequestedByUserId INT NOT NULL,
        RequestedByName NVARCHAR(200) NOT NULL,
        Status INT NOT NULL CONSTRAINT DF_WorkflowTasks_Status DEFAULT(0),
        Priority INT NOT NULL CONSTRAINT DF_WorkflowTasks_Priority DEFAULT(0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WorkflowTasks_CreatedAt DEFAULT SYSUTCDATETIME(),
        DueAt DATETIME2 NULL,
        DecidedAt DATETIME2 NULL,
        DecidedByUserId INT NULL,
        DecidedByUserName NVARCHAR(200) NULL,
        LastComment NVARCHAR(1000) NULL
    );
    CREATE INDEX IX_WorkflowTasks_Assignee ON dbo.WorkflowTasks(CompanyId, AssignedUserId, Status, DueAt);
    CREATE INDEX IX_WorkflowTasks_Document ON dbo.WorkflowTasks(CompanyId, Module, DocumentReference);
END;
GO

IF OBJECT_ID('dbo.WorkflowHistory','U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkflowHistory
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowHistory PRIMARY KEY,
        WorkflowTaskId BIGINT NOT NULL,
        CompanyId INT NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        UserId INT NOT NULL,
        UserName NVARCHAR(200) NOT NULL,
        Comment NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WorkflowHistory_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_WorkflowHistory_Task FOREIGN KEY (WorkflowTaskId) REFERENCES dbo.WorkflowTasks(Id)
    );
    CREATE INDEX IX_WorkflowHistory_Task ON dbo.WorkflowHistory(WorkflowTaskId, CreatedAt);
END;
GO
