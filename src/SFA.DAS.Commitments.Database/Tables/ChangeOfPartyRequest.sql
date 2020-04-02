CREATE TABLE [dbo].[ChangeOfPartyRequest]
(
	[Id] BIGINT NOT NULL IDENTITY PRIMARY KEY,
	[ApprenticeshipId] BIGINT NOT NULL,
	[ChangeOfPartyType] TINYINT NOT NULL,
	[OriginatingParty] TINYINT NOT NULL,
	[AccountId] BIGINT NULL,
	[ProviderId] BIGINT NULL,
	[Price] INT NOT NULL,
	[StartDate] DATETIME NOT NULL,
	[EndDate] DATETIME NULL,
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[Status] TINYINT NOT NULL DEFAULT(0)
	CONSTRAINT [FK_ChangeOfPartyRequest_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]),
	CONSTRAINT [FK_ChangeOfPartyRequest_NewAccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
	CONSTRAINT [FK_ChangeOfPartyRequest_NewProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers]([Ukprn])
)


