CREATE TABLE [dbo].[Framework]
(
    [Id] VARCHAR(25) NOT NULL PRIMARY KEY,
    [FrameworkCode] INT NOT NULL,
    [FrameworkName] VARCHAR(500) NOT NULL,    
    [Level] TINYINT NOT NULL,
    [PathwayCode] INT NOT NULL,
    [PathwayName] VARCHAR(500) NOT NULL,
    [ProgrammeType] INT NOT NULL,
    [Title] VARCHAR(500) NOT NULL,
    [Duration] INT NOT NULL,
    [MaxFunding] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL
)