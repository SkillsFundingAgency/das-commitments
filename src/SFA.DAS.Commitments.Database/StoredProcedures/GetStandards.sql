CREATE PROCEDURE [dbo].[GetStandards]
	
AS
BEGIN
	
	SELECT 
        s.LarsCode as Id,
        s.*,
        f.* 
    FROM 
        [dbo].[Standard] s 
    INNER JOIN 
        [dbo].[StandardFunding] f 
    ON 
        s.LarsCode = f.Id 
    WHERE s.IsLatestVersion = 1
	
END
