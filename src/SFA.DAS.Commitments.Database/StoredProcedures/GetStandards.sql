CREATE PROCEDURE [dbo].[GetStandards]
	
AS
BEGIN
	
	SELECT 
        s.*,
        f.* 
    FROM 
        [dbo].[Standard] s 
    INNER JOIN 
        [dbo].[StandardFunding] f 
    ON 
        s.LarsCode = f.Id 
	
END
