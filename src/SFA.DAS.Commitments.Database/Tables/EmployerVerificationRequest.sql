CREATE TABLE [dbo].[EmployerVerificationRequest]
(
    [ApprenticeshipId] BIGINT NOT NULL,
    [Created] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [Updated] DATETIME2 NULL,
    [LastCheckedDate] DATETIME2 NULL,
    [Status] SMALLINT NOT NULL,
    [Notes] NVARCHAR(1000) NULL,
    CONSTRAINT [PK_EmployerVerificationRequest] PRIMARY KEY ([ApprenticeshipId]),
    CONSTRAINT [FK_EmployerVerificationRequest_Apprenticeship]
        FOREIGN KEY ([ApprenticeshipId]) REFERENCES [dbo].[Apprenticeship]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_EmployerVerificationRequest_Status_Updated]
    ON [dbo].[EmployerVerificationRequest] ([Status], [Updated])
    INCLUDE ([ApprenticeshipId], [Created], [LastCheckedDate], [Notes])
    WITH (ONLINE = ON)
GO
