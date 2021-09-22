-- This Applies retrospective StandardUId and TrainingCourseVersion based on Startdate in Apprenticeship table
-- And sets TrainingCourseVersionConfirmed = 1 where there could only be one Standard version associated with that LARS code
-- otherwise will set TrainingCourseVersionConfirmed = 0 as there could be more than one version, as this is an estimate

-- where there could have only been one version can confirm version
-- NOTE version based on LarsCode to handle multi-lars code standards, where only one version per lars code
DECLARE 
@trigger_enabled varchar(10) = 'disable';

PRINT N'Patch historical Version in Apprenticeship - Started.';

-- disable the UpdateOn trigger, if it exists and is enabled
IF EXISTS ( SELECT  *
            FROM    sys.triggers
            WHERE   name = 'Trg_Apprenticeship_Update'
            AND is_disabled = 0 ) 
BEGIN
    PRINT N'Disabling trigger Trg_Apprenticeship_Update.';
    SET @trigger_enabled = 'enable';
    EXEC('ALTER TABLE [dbo].[Apprenticeship] DISABLE TRIGGER Trg_Apprenticeship_Update');
END

SELECT @trigger_enabled 


-- where there could have only been one version can confirm version
-- NOTE version based on LarsCode to handle multi-lars code standards, where only one version per lars code
MERGE INTO Apprenticeship apmaster
USING (
    SELECT ap1.id, st3.Version TrainingCourseVersion, 1 TrainingCourseVersionConfirmed, st3.StandardUId 
    FROM Apprenticeship ap1
    JOIN (
        SELECT * FROM (
            SELECT st1.Version, st1.StandardUId, COUNT(*) OVER (PARTITiON BY LarsCode) Versions, CONVERT(varchar,st1.LarsCode) TrainingCode
            FROM Standard st1
            WHERE LarsCode != 0
        ) st2 WHERE versions = 1 
    ) st3 on st3.TrainingCode = ap1.TrainingCode
    WHERE ap1.TrainingType = 0
      AND ap1.StandardUId IS NULL
) upd
ON ( apmaster.Id = upd.Id )
WHEN MATCHED THEN UPDATE
SET apmaster.TrainingCourseVersion = upd.TrainingCourseVersion 
   ,apmaster.TrainingCourseVersionConfirmed = upd.TrainingCourseVersionConfirmed
   ,apmaster.StandardUId = upd.StandardUId;


-- where could be more than one version then need to guess, but cannot confirm.
-- NOTE versions based on StandardReference, linked to Apprenticeship via LarsCode
MERGE INTO Apprenticeship apmaster
USING (
    SELECT ap1.id,
        (SELECT StandardUid FROM (
             SELECT row_number() OVER (PARTITION BY Ifatereferencenumber ORDER BY VersionMajor, VersionMinor) seq, StandardUid FROM Standard WHERE CONVERT(varchar,LarsCode) = ap1.TrainingCode
                AND (
                    VersionLatestStartDate IS NULL OR EOMONTH(VersionLatestStartDate) >= EOMONTH(ISNULL(ap1.StartDate,ap1.createdOn) ) 
                    OR (IsLatestVersion = 1 AND EOMONTH(VersionLatestStartDate) < EOMONTH(ISNULL(ap1.StartDate,ap1.createdOn)) )
                )
            ) st1 WHERE seq = 1
        ) AS StandardUID,
        (SELECT Version FROM (
             SELECT row_number() OVER (PARTITION BY Ifatereferencenumber ORDER BY VersionMajor, VersionMinor) seq, Version FROM Standard WHERE CONVERT(varchar,LarsCode) = ap1.TrainingCode
                AND (
                    VersionLatestStartDate IS NULL OR EOMONTH(VersionLatestStartDate) >= EOMONTH(ISNULL(ap1.StartDate,ap1.createdOn) ) 
                    OR (IsLatestVersion = 1 AND EOMONTH(VersionLatestStartDate) < EOMONTH(ISNULL(ap1.StartDate,ap1.createdOn)) )
                )
            ) st1 WHERE seq = 1
        ) AS TrainingCourseVersion,
        0 TrainingCourseVersionConfirmed
    FROM Apprenticeship ap1
    WHERE ap1.TrainingType = 0
      AND ap1.StandardUID IS NULL
      AND ap1.TrainingCode IS NOT NULL
 ) upd
ON ( apmaster.Id = upd.Id )
WHEN MATCHED AND upd.StandardUId IS NOT NULL THEN UPDATE
SET apmaster.TrainingCourseVersion = upd.TrainingCourseVersion 
   ,apmaster.TrainingCourseVersionConfirmed = upd.TrainingCourseVersionConfirmed
   ,apmaster.StandardUId = upd.StandardUId;

-- reset the UpdateOn trigger, if it exists and is enabled
IF @trigger_enabled = 'enable' 
BEGIN
    PRINT N'Enabling trigger Trg_Apprenticeship_Update.';
    EXEC('ALTER TABLE [dbo].[Apprenticeship] ENABLE TRIGGER Trg_Apprenticeship_Update');
END

PRINT N'Patch historical Version in Apprenticeship - Completed.';

GO