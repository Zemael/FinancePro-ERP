IF OBJECT_ID('dbo.SystemSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemSettings
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SystemSettings PRIMARY KEY,
        CompanyId INT NOT NULL,
        Category NVARCHAR(80) NOT NULL,
        [Key] NVARCHAR(120) NOT NULL,
        [Value] NVARCHAR(MAX) NULL,
        DataType NVARCHAR(80) NOT NULL CONSTRAINT DF_SystemSettings_DataType DEFAULT ('String'),
        Description NVARCHAR(300) NULL,
        IsEditable BIT NOT NULL CONSTRAINT DF_SystemSettings_IsEditable DEFAULT (1),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_SystemSettings_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedBy INT NULL,
        CONSTRAINT UQ_SystemSettings_Company_Category_Key UNIQUE (CompanyId, Category, [Key])
    );

    CREATE INDEX IX_SystemSettings_Company_Category ON dbo.SystemSettings (CompanyId, Category);
END;
GO
