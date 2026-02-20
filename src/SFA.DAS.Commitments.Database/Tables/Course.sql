CREATE TABLE [dbo].[Course]
(
    [LarsCode] VARCHAR(20) NOT NULL,
    [Title] NVARCHAR(500) NOT NULL,
    [Level] VARCHAR(20) NOT NULL,
    [LearningType] NVARCHAR(50) NULL,
    [MaxFunding] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL,
    CONSTRAINT [PK_Course] PRIMARY KEY CLUSTERED ([LarsCode] ASC)
);
GO
