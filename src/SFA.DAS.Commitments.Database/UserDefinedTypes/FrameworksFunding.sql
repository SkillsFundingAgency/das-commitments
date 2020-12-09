CREATE TYPE [dbo].[FrameworksFunding] AS TABLE
(
    [Id] VARCHAR(25) NOT NULL PRIMARY KEY,
    [FundingCap] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL
)