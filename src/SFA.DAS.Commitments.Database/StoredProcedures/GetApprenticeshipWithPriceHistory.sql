CREATE PROCEDURE [dbo].[GetApprenticeshipWithPriceHistory]
(
	@id BIGINT,
	@now DATETIME
)
AS 

SELECT 
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
	left join PriceHistory p on p.ApprenticeshipId = s.Id
	WHERE
	s.Id = @id
