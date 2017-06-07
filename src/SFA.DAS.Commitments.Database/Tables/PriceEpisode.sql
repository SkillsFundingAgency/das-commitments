CREATE TABLE [dbo].[PriceEpisode]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[ApprenticeshipId] BIGINT NOT NULL, 
	[Cost] DECIMAL NOT NULL,
	[FromDate] DateTime NOT NULL,
	[EndDate] Date NULL,
	CONSTRAINT [FK_PriceEpisode_Apprenticeship] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_PriceEpisode_ApprenticeshipId] ON [dbo].[PriceEpisode] ([ApprenticeshipId]) INCLUDE ([Cost], [FromDate], [EndDate]) WITH (ONLINE = ON)
