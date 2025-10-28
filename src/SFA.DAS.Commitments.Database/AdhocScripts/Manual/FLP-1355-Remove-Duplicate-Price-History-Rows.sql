BEGIN TRAN;

DECLARE @CommitmentIds TABLE (Id BIGINT);
INSERT INTO @CommitmentIds (Id)
--Scope the query to the affected Cohorts, could do this one at a time if preferred
VALUES (1733891), (1661951), (1733919), (1735857), (1736217), (1736218), (1736220), (1736223), (1736226);

--Select duplicates into temp table
SELECT 
    ph.Id,
    a.CommitmentId,
    ph.ApprenticeshipId,
    ph.FromDate,
    ph.ToDate,
    ph.Cost,
    ph.TrainingPrice,
    ph.AssessmentPrice,
    ROW_NUMBER() OVER (
        PARTITION BY 
            ph.ApprenticeshipId,
            ph.FromDate,
            ph.ToDate,
            ph.Cost,
            ph.TrainingPrice,
            ph.AssessmentPrice
        ORDER BY ph.Id
    ) AS RowNum
INTO #Duplicates
FROM dbo.PriceHistory ph
INNER JOIN dbo.Apprenticeship a ON a.Id = ph.ApprenticeshipId
INNER JOIN (
    SELECT 
        a.CommitmentId,
        ph.ApprenticeshipId,
        ph.FromDate,
        ph.ToDate,
        ph.Cost,
        ph.TrainingPrice,
        ph.AssessmentPrice
    FROM dbo.PriceHistory ph
    INNER JOIN dbo.Apprenticeship a ON a.Id = ph.ApprenticeshipId
    WHERE a.CommitmentId IN (SELECT Id FROM @CommitmentIds)
    GROUP BY 
        a.CommitmentId,
        ph.ApprenticeshipId,
        ph.FromDate,
        ph.ToDate,
        ph.Cost,
        ph.TrainingPrice,
        ph.AssessmentPrice
    HAVING COUNT(*) > 1
) dg
    ON dg.ApprenticeshipId = ph.ApprenticeshipId
    AND dg.FromDate = ph.FromDate
    AND ISNULL(dg.ToDate, '9999-12-31') = ISNULL(ph.ToDate, '9999-12-31')
    AND dg.Cost = ph.Cost
    AND ISNULL(dg.TrainingPrice, -1) = ISNULL(ph.TrainingPrice, -1)
    AND ISNULL(dg.AssessmentPrice, -1) = ISNULL(ph.AssessmentPrice, -1);

--Summary of duplicate records to delete
SELECT * FROM #Duplicates WHERE RowNum > 1;

--Delete duplicates
DELETE ph
FROM dbo.PriceHistory ph
INNER JOIN #Duplicates d ON ph.Id = d.Id
WHERE d.RowNum > 1;

--Output all price history rows for cohorts (there should be no duplicates left in here just the remaining valid data)
SELECT *
FROM Apprenticeship a
INNER JOIN PriceHistory ph ON ph.ApprenticeshipId = a.Id
WHERE a.CommitmentId IN (SELECT Id FROM @CommitmentIds);

DROP TABLE #Duplicates;


ROLLBACK TRAN;
-- COMMIT TRAN;
