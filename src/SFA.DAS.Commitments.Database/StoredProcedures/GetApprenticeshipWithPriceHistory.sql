CREATE PROCEDURE [dbo].[GetApprenticeshipWithPriceHistory]
(
	@id BIGINT,
	@now DATETIME
)
AS 

SELECT 
	A.Email,
	CASE 
		WHEN A.Email IS NULL THEN 'N/A'
		WHEN ACS.CommitmentsApprovedOn IS NULL THEN 'Unconfirmed'
		WHEN ACS.ApprenticeshipConfirmedOn IS NOT NULL THEN 'Confirmed'
		WHEN ACS.ConfirmationOverdueOn < @now THEN 'Overdue'
		ELSE 'Unconfirmed'
	END AS ConfirmationStatusDescription,
	s.*,
	CASE
		WHEN
			s.PaymentStatus = 0
		THEN
			s.Cost
		ELSE
			(
			SELECT TOP 1 Cost FROM PriceHistory WHERE ApprenticeshipId = s.Id
				AND ( 
					-- If started take if now with a PriceHistory or the last one (NULL end date)
					( s.StartDate <= @now
					  AND ( 
						( FromDate <= @now AND ToDate >= FORMAT(@now,'yyyMMdd')) 
						  OR ToDate IS NULL
						)
					)
					-- If not started take the first one
					OR (s.StartDate > @now) 
				)
				ORDER BY FromDate
			 )
	END AS 'Cost',
	p.*
	FROM ApprenticeshipSummary s
	join Apprenticeship A ON A.Id = s.Id
	left join ApprenticeshipConfirmationStatus ACS ON ACS.ApprenticeshipId = A.Id
	left join PriceHistory p on p.ApprenticeshipId = s.Id
	WHERE
	s.Id = @id
