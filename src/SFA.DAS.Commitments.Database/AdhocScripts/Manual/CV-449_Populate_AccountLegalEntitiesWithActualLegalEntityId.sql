/*
Script to populate Account/Legal Entity data for Approvals from MA source data

Instructions for use:
1. Turn on SQL CMD mode and Results to Text and max characters in text output to 8192 (query->query options->results->text->Maximum number of characters displayed in each column)
2. Execute this script against MA employer_account database
3. Execute the resulting script against commitments database

Expectations:
~5 minutes on employer_account database
~?? seconds on commitments db
70k+ LegalEntities updated
*/


SET NOCOUNT ON

declare @MAXINSERT int = 1000 --insert values() batch size (cannot be more than 1000)

--Some table var declarations
print 'declare @AccountLegalEntitiesUpdate table ([AccountLegalEntityId] bigint,[ActualLegalEntityId] bigint)'

BEGIN TRY

	--AccountLegalEntities
	select
	case (ROW_NUMBER() OVER (ORDER BY ale.Id) % @MAXINSERT)
	when 1 then CONVERT(nvarchar(max),'insert into @AccountLegalEntitiesUpdate ([AccountLegalEntityId],[ActualLegalEntityId]) values') + char(13) + char(10) else '' end +
	CONVERT(NVARCHAR(MAX),' (' + convert(varchar,ale.[Id]) + ', '
		+ convert(NVARCHAR(MAX),ale.[LegalEntityId])
	+ ')')  + 
	case when
		((ROW_NUMBER() OVER (ORDER BY ale.Id) % @MAXINSERT = 0)
		OR (ROW_NUMBER() OVER (ORDER BY ale.Id) = (select count(1) from [employer_account].[AccountLegalEntity] where PublicHashedId is not null and Deleted is null)))
	then CONVERT(NVARCHAR(MAX), '') else CONVERT(NVARCHAR(MAX),',') end
	from [employer_account].[AccountLegalEntity] ale
	where ale.PublicHashedId is not null
	and ale.Deleted is null
	order by ale.Id asc

	--Final inserts
	print '
	BEGIN TRANSACTION
	'

	print '
	UPDATE AccountLegalEntities SET 
		[MaLegalEntityId] = T.[ActualLegalEntityId] 
	FROM AccountLegalEntities ALE
	INNER JOIN @AccountLegalEntitiesUpdate T ON ALE.AccountLegalEntityId = T.AccountLegalEntityId

	print ''updated '' + convert(varchar,@@ROWCOUNT) + '' AccountLegalEntities''
	print ''Completed''

	UPDATE AccountLegalEntities SET 
		[MaLegalEntityId] = 0
	WHERE [MaLegalEntityId] IS NULL

	print ''zeroed '' + convert(varchar,@@ROWCOUNT) + '' AccountLegalEntities Not matched''


	COMMIT TRANSACTION
	'

END TRY
BEGIN CATCH
	PRINT 'Problem, there are blank public hashed Id'
END CATCH




