CREATE PROCEDURE [dbo].[ImportStandards]
    @standards [dbo].[Standards] READONLY
AS
    MERGE Standard as target
    USING (SELECT StandardUId,
                  LarsCode,
                  IFateReferenceNumber,
                  [Version],
                  Title,
                  [Level],
                  Duration,
                  MaxFunding,
                  EffectiveFrom,
                  EffectiveTo,
                  VersionMajor,
                  VersionMinor,
                  StandardPageUrl,
                  [Status],
                  IsLatestVersion,
                  VersionEarliestStartDate,
                  VersionLatestStartDate
           FROM @standards) AS source
    ON target.StandardUId = source.StandardUId
    WHEN MATCHED
        THEN
        UPDATE
        SET 
            target.LarsCode      = source.LarsCode,
            target.IFateReferenceNumber = source.IFateReferenceNumber,
            target.[Version]     = source.[Version],
            target.Title         = source.Title,
            target.[Level]       = source.[Level],
            target.Duration      = source.Duration,
            target.MaxFunding    = source.MaxFunding,
            target.EffectiveFrom = source.EffectiveFrom,
            target.EffectiveTo   = source.EffectiveTo,
            target.VersionMajor  = source.VersionMajor,
            target.VersionMinor  = source.VersionMinor,
            target.StandardPageUrl = source.StandardPageUrl,
            target.[Status]        = source.[Status],
            target.IsLatestVersion = source.IsLatestVersion,
            target.VersionEarliestStartDate = source.VersionEarliestStartDate,
            target.VersionLatestStartDate = source.VersionLatestStartDate
            
    WHEN NOT MATCHED BY TARGET then
        INSERT (StandardUId, LarsCode, IFateReferenceNumber, [Version], Title, [Level], Duration, MaxFunding, EffectiveFrom, EffectiveTo, VersionMajor, VersionMinor, StandardPageUrl, [Status], IsLatestVersion, VersionEarliestStartDate, VersionLatestStartDate)
        VALUES (source.StandardUId, source.LarsCode, source.IFateReferenceNumber, source.[Version], source.Title, source.[Level], source.Duration, source.MaxFunding,
                source.EffectiveFrom, source.EffectiveTo, source.VersionMajor, source.VersionMinor, source.StandardPageUrl, source.[Status], source.IsLatestVersion, source.VersionEarliestStartDate, source.VersionLatestStartDate );