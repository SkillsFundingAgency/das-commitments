CREATE PROCEDURE [dbo].[ImportStandardsFunding]
    @standardsFunding [dbo].[StandardsFunding] READONLY
AS
    MERGE StandardFunding as target
    USING (SELECT Id,
                  FundingCap,
                  EffectiveFrom,
                  EffectiveTo
           FROM @standardsFunding) AS source
    ON target.Id = source.Id AND target.EffectiveFrom = source.EffectiveFrom
    WHEN MATCHED
        THEN
        UPDATE
        SET target.FundingCap = source.FundingCap,
            target.EffectiveTo = source.EffectiveTo
    WHEN NOT MATCHED BY TARGET then
        INSERT (Id, FundingCap, EffectiveFrom, EffectiveTo)
        VALUES (source.Id, source.FundingCap, source.EffectiveFrom, source.EffectiveTo);
