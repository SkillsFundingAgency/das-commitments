CREATE TABLE [dbo].[PriceHistory]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[ApprenticeshipId] BIGINT NOT NULL, 
	[Cost] DECIMAL NOT NULL,
	[TrainingPrice] DECIMAL NULL,
	[AssessmentPrice] DECIMAL NULL,
	[FromDate] DateTime NOT NULL,
	[ToDate] DateTime NULL,
	CONSTRAINT [FK_PriceHistory_Apprenticeship] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_PriceHistory_ApprenticeshipId] ON [dbo].[PriceHistory] ([ApprenticeshipId]) INCLUDE ([Cost], [FromDate], [ToDate]) WITH (ONLINE = ON)
