
/*
Script to manually insert missing providers
*/

BEGIN TRANSACTION;

INSERT INTO Providers (Ukprn, [Name], Created)
SELECT C.ProviderId, C.ProviderName, GETDATE()
  FROM [dbo].[Commitment] C
  LEFT JOIN Providers P ON C.ProviderId = P.Ukprn
  WHERE P.Ukprn IS NULL AND C.ProviderName IS NOT NULL
  GROUP BY C.ProviderId, C.ProviderName;

COMMIT TRANSACTION;