CREATE TABLE [dbo].[ChangeOfPartyRequest]
(
	[Id] BIGINT NOT NULL IDENTITY PRIMARY KEY,
	[ApprenticeshipId] BIGINT NOT NULL,
	[ChangeOfPartyType] TINYINT NOT NULL,
	[OriginatingParty] SMALLINT NOT NULL,
	[AccountLegalEntityId] BIGINT NULL,
	[ProviderId] BIGINT NULL,
	[Price] INT NULL,
	[StartDate] DATETIME NULL,
	[EndDate] DATETIME NULL,
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[Status] TINYINT NOT NULL DEFAULT(0),
	[RowVersion] ROWVERSION NOT NULL,
	[LastUpdatedOn] DATETIME2 DEFAULT GETDATE() NOT NULL,
	[CohortId] BIGINT NULL,
	[ActionedOn] DATETIME2 NULL,
	[NewApprenticeshipId] BIGINT NULL,
	[EmploymentPrice] INT NULL, 
    [EmploymentEndDate] DATETIME2 NULL, 
    CONSTRAINT [FK_ChangeOfPartyRequest_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]),
	CONSTRAINT [FK_ChangeOfPartyRequest_AccountLegalEntityId] FOREIGN KEY ([AccountLegalEntityId]) REFERENCES [AccountLegalEntities]([Id]),
	CONSTRAINT [FK_ChangeOfPartyRequest_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers]([Ukprn]),
	CONSTRAINT [FK_ChangeOfPartyRequest_CohortId] FOREIGN KEY ([CohortId]) REFERENCES [Commitment]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IDX_ChangeOfPartyRequest_Apprenticeship] ON [dbo].[ChangeOfPartyRequest] ([ApprenticeshipId], [NewApprenticeshipId], [Status])
GO