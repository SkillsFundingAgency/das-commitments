CREATE PROCEDURE [dbo].[GetApprenticeshipsByULN]
(
  @ULN NVARCHAR(50)
)
AS

SELECT 
	s.*
	FROM ApprenticeshipSummary s
	WHERE s.ULN = @ULN
	ORDER BY s.FirstName asc, s.LastName asc;

SELECT 
	COUNT(*)
	FROM ApprenticeshipSummary s
	WHERE s.ULN = @ULN
