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
			a.PaymentStatus = 0
		THEN
			a.Cost
		ELSE
			(
			SELECT TOP 1 Cost FROM PriceHistory WHERE ApprenticeshipId = a.Id
				AND ( 
					-- If started take if now with a PriceHistory or the last one (NULL end date)
					( a.StartDate <= @now
					  AND ( 
						( FromDate <= @now AND ToDate >= FORMAT(@now,'yyyMMdd')) 
						  OR ToDate IS NULL
						)
					)
					-- If not started take the first one
					OR (a.StartDate > @now) 
				)
				ORDER BY FromDate
			 )
	END AS 'Cost',
	p.*
	FROM ApprenticeshipSummary s
	inner join Apprenticeship a on a.Id = s.Id
	left join PriceHistory p on p.ApprenticeshipId = s.Id
	WHERE
	s.Id = @id

