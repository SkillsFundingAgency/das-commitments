CREATE TYPE [dbo].[Courses] AS TABLE
(
    [LarsCode] VARCHAR(20) NOT NULL,
    [Title] NVARCHAR(500) NOT NULL,
    [Level] VARCHAR(20) NOT NULL,
    [LearningType] NVARCHAR(50) NULL,
    [MaxFunding] INT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL
)