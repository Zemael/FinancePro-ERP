IF OBJECT_ID('dbo.HumanResourcesAttendance','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesAttendance(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, EmployeeId int NOT NULL,
  WorkDate date NOT NULL, EntryTime time(0) NOT NULL, ExitTime time(0) NULL,
  RegularHours decimal(8,2) NOT NULL DEFAULT 0, OvertimeHours decimal(8,2) NOT NULL DEFAULT 0,
  OvertimeAmount decimal(18,2) NOT NULL DEFAULT 0, Status nvarchar(30) NOT NULL DEFAULT N'Pendente',
  Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_HumanResourcesAttendance_Employee FOREIGN KEY(EmployeeId) REFERENCES dbo.HumanResourcesEmployees(Id),
  CONSTRAINT UQ_HumanResourcesAttendance_Day UNIQUE(CompanyId,EmployeeId,WorkDate),
  CONSTRAINT CK_HumanResourcesAttendance_Values CHECK(RegularHours>=0 AND OvertimeHours>=0 AND OvertimeAmount>=0),
  CONSTRAINT CK_HumanResourcesAttendance_Status CHECK(Status IN(N'Pendente',N'Aprovado',N'Rejeitado'))
 );
 CREATE INDEX IX_HumanResourcesAttendance_Period ON dbo.HumanResourcesAttendance(CompanyId,EmployeeId,WorkDate,Status,Active);
END;
