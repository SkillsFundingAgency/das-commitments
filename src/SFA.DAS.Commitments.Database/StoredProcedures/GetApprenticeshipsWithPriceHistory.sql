﻿CREATE PROCEDURE [dbo].[GetApprenticeshipsWithPriceHistory]
(
	@now DATETIME,
	@providerId BIGINT = NULL,
	@employerId BIGINT = NULL,
	@searchTerm NVARCHAR(200) = '',
	@uln BIGINT = NULL
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
			SELECT TOP 1 Cost
				FROM PriceHistory
				WHERE ApprenticeshipId = a.Id
				AND (
					(FromDate <= @now AND ToDate >= FORMAT(@now,'yyyMMdd')) 
					OR ToDate IS NULL
				)
				ORDER BY FromDate
			 )
	END AS 'Cost',
	p.*
	FROM ApprenticeshipSummary s
	inner join Apprenticeship a on a.Id = s.Id
	left join PriceHistory p on p.ApprenticeshipId = s.Id
	WHERE
		(@providerId IS NULL OR @providerId = s.ProviderId)
	AND
		(@employerId IS NULL OR @employerId = s.EmployerAccountId)
	AND
		a.PaymentStatus <> 5 --Not deleted
	AND
	    (
			(@uln IS NOT NULL AND a.ULN = @uln)
			OR
			(@uln IS NULL 
			AND
			a.FirstName + ' ' + a.LastName like '%' + @searchTerm + '%'
			)
		)
	ORDER BY a.FirstName asc, a.LastName asc;

SELECT COUNT(*) FROM ApprenticeshipSummary
WHERE
	(@providerId IS NULL OR ProviderId = @providerId)
AND
	(@employerId IS NULL OR EmployerAccountId = @employerId)
AND
	PaymentStatus <> 5 --Not deleted