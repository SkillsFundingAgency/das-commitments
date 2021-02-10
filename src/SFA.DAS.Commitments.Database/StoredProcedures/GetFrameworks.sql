CREATE PROCEDURE [dbo].[GetFrameworks]
	
AS
BEGIN
	
	SELECT 
        fr.*,
        f.* 
    FROM 
        [dbo].[Framework] fr 
    INNER JOIN 
        [dbo].[FrameworkFunding] f 
    ON 
        fr.Id = f.Id 
	
END
