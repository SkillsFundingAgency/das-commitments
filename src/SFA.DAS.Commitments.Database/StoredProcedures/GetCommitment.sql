CREATE PROCEDURE [dbo].[GetCommitment]
	@commitmentId BIGINT
AS

SELECT 
	c.*,
	a.* 
FROM 
	[dbo].[CommitmentSummary] c 
LEFT JOIN 
	[dbo].[ApprenticeshipSummary] a 
ON 
	a.CommitmentId = c.Id 
WHERE 
	c.Id = @commitmentId 
AND 
	c.IsDeleted = 0

