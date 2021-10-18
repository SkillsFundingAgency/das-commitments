 --SELECT 'UPDATE Apprenticeship SET Email = ''' + A.Email + ''' WHERE ' + CONCAT('Id = ', R.CommitmentsApprenticeshipId) + ' AND Email <> ''' + A.Email + ''''
  SELECT 'IF EXISTS(SELECT * FROM Apprenticeship WHERE ' + CONCAT('Id = ', R.CommitmentsApprenticeshipId) + ' AND Email <> ''' + A.Email + ''') ' +  + CONCAT('PRINT ', R.CommitmentsApprenticeshipId)
  FROM Apprentice A
  LEFT JOIN Apprenticeship APPS ON APPS.ApprenticeId = A.Id  
  LEFT JOIN Revision R ON R.ApprenticeshipId = APPS.Id 
  WHERE R.CommitmentsApprenticeshipId IS NOT NULL




  -- REsults Should be inserted here
  /*
  

  BEGIN TRANSACTION


  -- Code here

  ROLLBACK 
  --COMMIT 

  */