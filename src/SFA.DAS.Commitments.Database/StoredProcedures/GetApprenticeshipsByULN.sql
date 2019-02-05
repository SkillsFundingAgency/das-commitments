CREATE PROCEDURE [dbo].[GetApprenticeshipsByULN]
(
  @ULN NVARCHAR(50),
  @hashedAccountId NVARCHAR(50)
)
AS

SELECT 
	s.*
	FROM ApprenticeshipSummary s
	WHERE s.ULN = @ULN
	AND S.Reference = @hashedAccountId
	
SELECT @@ROWCOUNT;
