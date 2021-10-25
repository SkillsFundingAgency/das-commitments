  SELECT 'UPDATE Apprenticeship SET Email = ''' + A.Email + ''' WHERE ' + CONCAT('Id = ', APPS.Id) + ' AND Email <> ''' + A.Email + '''' + CHAR(13)+CHAR(10) + 'UPDATE Apprenticeship SET EmailAddressConfirmed = 1 WHERE ' + CONCAT('Id = ', APPS.Id)
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