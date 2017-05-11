CREATE VIEW [dbo].[CommitmentSummaryWithMessages]
AS 

SELECT c.*, 
	m.CommitmentId, m.Text, m.CreatedDateTime, m.Author, m.CreatedBy
FROM 
	CommitmentSummary c
LEFT JOIN [Message] m ON m.CommitmentId = c.Id