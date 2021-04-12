SFA.DAS.CommitmentsV2
===========================

#### Context

Commitments Api V2 is one of a number of commitments products. You will probably want to also run the following locally:

* ProviderCommitments
* EmployerCommitments
* EmployerCommitmentv2
* Commitments API (v1)
* Provider Apprenticeship Service

### Getting started

1. Clone this repo, which includes both v1 and v2 API implementations: https://github.com/SkillsFundingAgency/das-commitments
2. In Visual Studio, publish the SFA.DAS.Commitments.Database project to your local (.) SQL Server instance.
3. Seed your new local db with the below script
4. Obtain cloud config for:
   * SFA.DAS.CommitmentsV2
   * SFA.DAS.Encoding
   * SFA.DAS.Reservations.Api.Client
5. Start Microsoft Azure Storage Emulator
6. Run the SFA.DAS.CommitmentsV2.Api project
7. Run the SFA.DAS.CommitmentsV2.MessageHandlers project

The API exposes swagger documentation when accessed on: https://localhost:5011/

#### Seeded data

Run the following sql against the database in order to create some basic startup data.

```sql

insert into Accounts (Id, HashedId, PublicHashedId, Name, Created, Updated, LevyStatus) values (8194, 'VN48RP', '7YRV9B', 'MegaCorp Inc', GETDATE(), GETDATE(), 1)
insert into AccountLegalEntities (Id, PublicHashedId, AccountId, [Name], [Address],[OrganisationType],[LegalEntityId], Created, Updated, MaLegalEntityId) values (2817, 'XEGE5X', 8194, 'Mega Corp Pharmaceuticals', '1 MegaCorp Way', 1, '736281', GETDATE(), GETDATE(), 2817)
insert into AccountLegalEntities (Id, PublicHashedId, AccountId, [Name],[Address],[OrganisationType],[LegalEntityId], Created, Updated, MaLegalEntityId) values (2818, 'XJGZ72', 8194, 'Mega Corp Bank', '2 MegaCorp Way', 1, '372628', GETDATE(), GETDATE(), 2818)
insert into Accounts (Id, HashedId, PublicHashedId, Name, Created, Updated) values (30060, 'VNR6P9', '7Y94BK', 'Rapid Logistics Co Ltd', GETDATE(), GETDATE())
insert into AccountLegalEntities (Id, PublicHashedId, AccountId, [Name],[Address],[OrganisationType],[LegalEntityId],Created, Updated, MaLegalEntityId) values (645, 'X9JE72', 30060, 'Rapid Logistics Co Ltd', '1 High Street', 1, '06344082', GETDATE(), GETDATE(), 645)
insert into Accounts (Id, HashedId, PublicHashedId, Name, Created, Updated) values (36853, 'MBWGGD', '78KDD4', 'Positivity Ltd', GETDATE(), GETDATE())
insert into AccountLegalEntities (Id, PublicHashedId, AccountId, [Name],[Address],[OrganisationType],[LegalEntityId],Created, Updated, MaLegalEntityId) values (701, 'XKD5Z2', 36853, 'Positivity Ltd', '1 High Street', 1, '70110101', GETDATE(), GETDATE(), 701)
insert into Providers(Ukprn, [Name], [Created]) values (10005077,'Train-U-Good Corporation', GETDATE())
insert into Providers(Ukprn, [Name], [Created]) values (10038368,'Amazing Training Ltd', GETDATE())
insert into Providers(Ukprn, [Name], [Created]) values (10000896,'Like a Pro Education Inc.', GETDATE())
````

#### Jobs

To get Course information it is necessary to run the **ImportStandardsJob** which will require you to have a subscription key for the commitments outer api on [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints). If you are not part of the ESFA organisation then you can follow the readme for running the commitments outer api locally. The commitments V2 configuration key should then be updated with the following:
```
"ApprovalsOuterApiConfiguration":{"Key":"APIM-KEY","BaseUrl":"https://[APIM-BASEURL]/"}
```
