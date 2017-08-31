CREATE PROCEDURE [dbo].[GetCommitment]
	@commitmentId BIGINT
AS

SELECT 
	c.*,
	a.* 
FROM 
	[dbo].[CommitmentSummary] c 
LEFT JOIN 
	[dbo].[Apprenticeship] a 
ON 
	a.CommitmentId = c.Id 
WHERE 
	c.Id = @commitmentId 
AND 
	c.CommitmentStatus <> 2 -- ignore deleted
