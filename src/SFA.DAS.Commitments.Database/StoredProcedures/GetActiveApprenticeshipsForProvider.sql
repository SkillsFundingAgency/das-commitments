CREATE PROCEDURE [dbo].[GetActiveApprenticeshipsForProvider]
	@id BIGINT
AS

SELECT 
	s.*
	FROM ApprenticeshipSummary s
	WHERE @id = s.ProviderId
	AND NOT s.PaymentStatus IN (0,5); --Not deleted or pre-approved
