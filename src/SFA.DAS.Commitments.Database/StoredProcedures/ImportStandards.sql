CREATE PROCEDURE [dbo].[ImportStandards] 
    @standards [dbo].[Standards] READONLY    
AS
    MERGE Standard as target
    USING (SELECT Id,
                  Title,
                  CourseName,
                  [Level],
                  Duration,
                  MaxFunding,
                  EffectiveFrom,
                  EffectiveTo
           FROM @standards) AS source
    ON target.Id = source.Id
    WHEN MATCHED
        THEN
        UPDATE
        SET target.Title         = source.Title,
            target.CourseName    = source.CourseName,
            target.[Level]       = source.[Level],
            target.Duration      = source.Duration,
            target.MaxFunding    = source.MaxFunding,
            target.EffectiveFrom = source.EffectiveFrom,
            target.EffectiveTo   = source.EffectiveTo
    WHEN NOT MATCHED BY TARGET then
        INSERT (Id, Title, CourseName, [Level], Duration, MaxFunding, EffectiveFrom, EffectiveTo)
        VALUES (source.Id, source.Title, source.CourseName, source.Level, source.Duration, source.MaxFunding,
                source.EffectiveFrom, source.EffectiveTo);