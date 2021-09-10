CREATE TYPE [dbo].[Standards] AS TABLE
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
    [VersionLatestStartDate] DATETIME NULL
)
