CREATE TYPE [dbo].[StandardsFunding] AS TABLE
(
    [Id] VARCHAR(25) NOT NULL,
    [FundingCap] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL
)