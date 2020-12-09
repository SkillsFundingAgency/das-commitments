CREATE TYPE [dbo].[Standards] AS TABLE
(
    [Id] INT NOT NULL PRIMARY KEY,
    [Title] VARCHAR(500) NOT NULL,
    [Level] TINYINT NOT NULL,
    [Duration] INT NOT NULL,
    [MaxFunding] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL
)
