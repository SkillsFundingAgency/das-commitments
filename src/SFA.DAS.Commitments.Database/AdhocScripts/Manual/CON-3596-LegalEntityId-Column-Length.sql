  /*
  CON-3596 -  Failure to handled AddedLegalEntityEvent due to OrganisationReference string length
  */
  
  ALTER TABLE [dbo].[AccountLegalEntities]
  ALTER COLUMN [LegalEntityId] NVARCHAR(100) NOT NULL; 