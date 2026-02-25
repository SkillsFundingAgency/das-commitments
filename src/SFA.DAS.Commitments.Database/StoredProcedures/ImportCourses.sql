CREATE PROCEDURE [dbo].[ImportCourses]
    @courses [dbo].[Courses] READONLY
AS
    MERGE Course as target
    USING (SELECT LarsCode,
                  Title,
                  [Level],
                  LearningType,
                  MaxFunding,
                  EffectiveFrom,
                  EffectiveTo
           FROM @courses) AS source
    ON target.LarsCode = source.LarsCode
    WHEN MATCHED
        THEN
        UPDATE
        SET
            target.Title         = source.Title,
            target.[Level]       = source.[Level],
            target.LearningType  = source.LearningType,
            target.MaxFunding    = source.MaxFunding,
            target.EffectiveFrom = source.EffectiveFrom,
            target.EffectiveTo   = source.EffectiveTo
    WHEN NOT MATCHED BY TARGET then
        INSERT (LarsCode, Title, [Level], LearningType, MaxFunding, EffectiveFrom, EffectiveTo)
        VALUES (source.LarsCode, source.Title, source.[Level], source.LearningType, source.MaxFunding,source.EffectiveFrom, source.EffectiveTo);