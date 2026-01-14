CREATE TABLE [dbo].[Standard]
(
    [StandardUId] NVARCHAR(20) NOT NULL,
    [LarsCode] INT NOT NULL,
    [IFateReferenceNumber] NVARCHAR(10) NOT NULL,
    [Version] NVARCHAR(10) NULL,
    [Title] VARCHAR(500) NOT NULL,
    [Level] TINYINT NOT NULL,
    [Duration] INT NOT NULL,
    [MaxFunding] INT NOT NULL,
    [EffectiveFrom] DATETIME NULL,
    [EffectiveTo] DATETIME NULL,
    [VersionMajor] INT NOT NULL DEFAULT 0, 
    [VersionMinor] INT NOT NULL DEFAULT 0, 
    [StandardPageUrl] NVARCHAR(500) NULL,
    [Status] NVARCHAR(50) NULL,
    [IsLatestVersion] BIT NOT NULL DEFAULT 0, 
    [VersionEarliestStartDate] DATETIME NULL,
    [VersionLatestStartDate] DATETIME NULL,
    [Route] NVARCHAR(500) NULL, 
    [ApprenticeshipType] VARCHAR(50) NULL
    CONSTRAINT [PK_Standards] PRIMARY KEY CLUSTERED (StandardUId ASC)
)
GO
CREATE NONCLUSTERED INDEX [IX_Standard_LarsCodeIsLatestVersion] ON [dbo].[Standard] ([LarsCode],[IsLatestVersion])
INCLUDE ([StandardUId], [IFateReferenceNumber], [Version], [Title], [Level], [Duration], [MaxFunding],
[EffectiveFrom], [EffectiveTo], [VersionMajor], [VersionMinor], [StandardPageUrl], [Status], [VersionEarliestStartDate], [VersionLatestStartDate], [Route])
GO