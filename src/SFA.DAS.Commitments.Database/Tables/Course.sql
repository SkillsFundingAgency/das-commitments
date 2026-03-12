CREATE TABLE [dbo].[Course]
(
    [LarsCode] VARCHAR(20) NOT NULL,
    [Title] NVARCHAR(500) NOT NULL,
    [Level] VARCHAR(20) NOT NULL,
    [LearningType] TINYINT NULL,
    [MaxFunding] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL,
    CONSTRAINT [PK_Course] PRIMARY KEY CLUSTERED ([LarsCode] ASC)
);
GO
