-- This Script needs to be run against das-prd-acomt-db and will geneerate a script to run on das-prd-comt-db, which prints out apprenticeships out of sync with CMAD
SELECT
	'IF EXISTS(SELECT * FROM Apprenticeship WHERE Id = ' + CONCAT(R.CommitmentsApprenticeshipId, '') + ' AND Email <> ''' + A.Email + ''' AND PaymentStatus NOT IN (3,4)) BEGIN ' +CHAR(13) +CHAR(10) +
	'PRINT ''--Email needs updating + ' + A.Email + ':' + CONCAT(R.CommitmentsApprenticeshipId, '') + '''' + CHAR(13) + CHAR(10) +
	'END'
FROM Apprentice A
JOIN Apprenticeship AA ON AA.ApprenticeId = A.Id
JOIN Revision R ON AA.Id = R.ApprenticeshipId
WHERE (SELECT COUNT(*) FROM ApprenticeEmailAddressHistory H WHERE H.ApprenticeId = A.Id) > 1


/*
This currently only identifies one apprenticeship in PROD which needs syncing in commitments

--Email needs updating + daniella-afonso@hotmail.co.uk:1439012

but some additional queries identified this apprenticeship also needing to be synced

--Email needs updating + nathanfgowers@icloud.com:1342371
*/

--on das-prd-comt-db We will need to run these

SELECT * FROM Apprenticeship WHERE Id IN (1342371,1439012)

UPDATE Apprenticeship SET
	Email = 'daniella-afonso@hotmail.co.uk' 
WHERE Id = 1439012

UPDATE Apprenticeship SET
	Email = 'nathanfgowers@icloud.com' 
WHERE Id = 1342371


