/*
Adhoc script to add AccountLegalEntities records for failed AddedLegalEntityEvent Events
*/

IF NOT EXISTS (Select 1 from [AccountLegalEntities] Where Id = 37132)
INSERT INTO [AccountLegalEntities] ([Id], [AccountId], [Address], [Created],  [LegalEntityId], [MaLegalEntityId], [Name], [OrganisationType], [PublicHashedId])
Values (37132,33793,'Wickham Street, Welling, Kent, DA16 3BP','2020-01-22T09:52:24','f5efecf6-e768-4a6f-a0a4-33e7305ba1ef',44196,'East Wickham Primary Academy',3,'XW4P44');


IF NOT EXISTS (Select 1 from [AccountLegalEntities] Where Id = 37434)
INSERT INTO [AccountLegalEntities] ([Id], [AccountId], [Address], [Created], [LegalEntityId], [MaLegalEntityId], [Name], [OrganisationType], [PublicHashedId])
VALUES (37434, 34128, 'Brecon Chase, Minster, Sheerness, Kent, ME12 2HX', '2020-01-27T13:19:13','e850bb4d-66aa-4c23-8cbe-02bd50016eb8', 44484, 'Minster in Sheppey Primary School', 3, '5ZEJRK'); 
