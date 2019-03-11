SFA.DAS.CommitmentsV2
===========================

#### Context

Commitments Api V2 is one of a number of commitments products. You will probably want to also run the following locally:

* ProviderCommitments
* EmployerCommitments
* Commitments API
* Events API


#### Configuration Requirements

* SFA.DAS.CommitmentsV2_v1.0




#### Seeded data

Run the following sql against the database in order to create some test data.

```sql

insert into Accounts (Id, HashedId, PublicHashedId, Name, Created, Updated) values (8194, 'RGB5G8', '4PGZEY', 'MegaCorp Inc', GETDATE(), GETDATE())
insert into AccountLegalEntities (Id, PublicHashedId, AccountId, [Name], [Address],[OrganisationType],[LegalEntityId], Created, Updated) values (2817, 'YZWX27', 8194, 'Mega Corp Pharmaceuticals', '1 MegaCorp Way', 1, '736281', GETDATE(), GETDATE())
insert into AccountLegalEntities (Id, PublicHashedId, AccountId, [Name],[Address],[OrganisationType],[LegalEntityId], Created, Updated) values (2818, '7N3MEY', 8194, 'Mega Corp Bank', '2 MegaCorp Way', 1, '372628', GETDATE(), GETDATE())
insert into Accounts (Id, HashedId, PublicHashedId, Name, Created, Updated) values (30060, 'R5W6WZ', '4NMEMR', 'Rapid Logistics Co Ltd', GETDATE(), GETDATE())
insert into AccountLegalEntities (Id, PublicHashedId, AccountId, [Name],[Address],[OrganisationType],[LegalEntityId],Created, Updated) values (645, '7N3MEY', 30060, 'Rapid Logistics Co Ltd', '1 High Street', 1, '06344082', GETDATE(), GETDATE())
insert into Providers(Ukprn, [Name], [Created]) values (10005077,'Train-U-Good Corporation', GETDATE())


````


