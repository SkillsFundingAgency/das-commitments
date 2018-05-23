CREATE PROCEDURE [dbo].[GetStatistics]
AS
SELECT
(
	SELECT COUNT(Id)
	FROM [Commitment]
) AS TotalCohorts,
(
	SELECT COUNT(Id)
	FROM [Apprenticeship]
) AS TotalApprenticeships,
(
	SELECT COUNT(Id)
	FROM [Apprenticeship]
	WHERE PaymentStatus = 1
) AS ActiveApprenticeships