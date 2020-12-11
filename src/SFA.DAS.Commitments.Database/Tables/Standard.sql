CREATE TABLE [dbo].[Standard]
(
    [Id] INT NOT NULL,
    [Title] VARCHAR(500) NOT NULL,
    [Level] TINYINT NOT NULL,
    [Duration] INT NOT NULL,
    [MaxFunding] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL,
    constraint PK_Standard PRIMARY KEY (Id),
)