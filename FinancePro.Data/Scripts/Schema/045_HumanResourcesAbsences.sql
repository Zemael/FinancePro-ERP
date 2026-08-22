IF OBJECT_ID('dbo.HumanResourcesAbsences','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesAbsences(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, EmployeeId int NOT NULL,
  AbsenceType nvarchar(40) NOT NULL, StartDate date NOT NULL, EndDate date NOT NULL,
  Days decimal(8,2) NOT NULL, Paid bit NOT NULL DEFAULT 1,
  DeductionAmount decimal(18,2) NOT NULL DEFAULT 0, Status nvarchar(30) NOT NULL DEFAULT N'Pendente',
  Reason nvarchar(200) NOT NULL, Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_HumanResourcesAbsences_Employee FOREIGN KEY(EmployeeId) REFERENCES dbo.HumanResourcesEmployees(Id),
  CONSTRAINT CK_HumanResourcesAbsences_Values CHECK(EndDate>=StartDate AND Days>0 AND DeductionAmount>=0),
  CONSTRAINT CK_HumanResourcesAbsences_Status CHECK(Status IN(N'Pendente',N'Aprovada',N'Rejeitada'))
 );
 CREATE INDEX IX_HumanResourcesAbsences_Period ON dbo.HumanResourcesAbsences(CompanyId,EmployeeId,StartDate,EndDate,Status,Active);
END;
