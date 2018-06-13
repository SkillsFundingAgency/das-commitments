CREATE PROCEDURE [dbo].[GetApprenticeshipsByULN]
(
  @ULN NVARCHAR(50)
)
AS

SELECT 
	s.*
	FROM ApprenticeshipSummary s
	WHERE s.ULN = @ULN;
	
SELECT @@ROWCOUNT;
