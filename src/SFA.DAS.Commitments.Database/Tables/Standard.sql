CREATE TABLE [dbo].[Standard]
(
    [Id] INT NOT NULL,
    [StandardUId] NVARCHAR(20) NULL,
    [Version] NVARCHAR(10) NULL,
    [Title] VARCHAR(500) NOT NULL,
    [Level] TINYINT NOT NULL,
    [Duration] INT NOT NULL,
    [MaxFunding] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL,
    CONSTRAINT [PK_Standards] PRIMARY KEY CLUSTERED ([Id] ASC)    
)