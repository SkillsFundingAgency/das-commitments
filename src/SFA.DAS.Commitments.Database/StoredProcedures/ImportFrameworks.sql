CREATE PROCEDURE [dbo].[ImportFrameworks] 
    @frameworks [dbo].[Frameworks] READONLY    
AS
    MERGE Framework as target
    USING (SELECT Id,
                  FrameworkCode,
                  FrameworkName,
                  [Level],
                  PathwayCode,
                  PathwayName,
                  ProgrammeType,
                  Title,
                  Duration,
                  MaxFunding,
                  EffectiveFrom,
                  EffectiveTo
           FROM @frameworks) AS source
    ON target.Id = source.Id
    WHEN MATCHED
        THEN
        UPDATE
        SET target.FrameworkCode         = source.FrameworkCode,
            target.FrameworkName    = source.FrameworkName,
            target.[Level]       = source.[Level],
            target.PathwayCode      = source.PathwayCode,
            target.PathwayName    = source.PathwayName,
            target.ProgrammeType    = source.ProgrammeType,
            target.Title    = source.Title,
            target.Duration    = source.Duration,
            target.MaxFunding    = source.MaxFunding,
            target.EffectiveFrom = source.EffectiveFrom,
            target.EffectiveTo   = source.EffectiveTo
    WHEN NOT MATCHED BY TARGET then
        INSERT (Id, FrameworkCode, FrameworkName, [Level], PathwayCode, PathwayName, ProgrammeType, Title, Duration, MaxFunding,
                  EffectiveFrom, EffectiveTo)
        VALUES (source.Id, source.FrameworkCode, source.FrameworkName, source.Level, source.PathwayCode, source.PathwayName, 
                source.ProgrammeType, source.Title, source.Duration, source.MaxFunding, source.EffectiveFrom, source.EffectiveTo);
                