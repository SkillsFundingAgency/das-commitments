# Commitments API #

Commitments API for the Digital Apprenticeship Service

For Commitments V2, see: https://github.com/SkillsFundingAgency/das-commitments/tree/master/src/CommitmentsV2


## Getting started Api (covering v1 and v2) ##
* Clone das-commitments repo
* Open das-commitments solution - build fails. This is due to a Slow Cheetah issue. To workaround, change build configuration to Release and build, then back to Debug and build. Solution will then build ok.
* Run the SFA.DAS.CommitmentsV2.Api project.
* Publish the database project to local db server (use default db name "SFA.DAS.Commitments.Database")
* Execute sql to seed data - see https://github.com/SkillsFundingAgency/das-commitments/tree/master/src/CommitmentsV2 
* Obtain cloud config - See below
* Run Storage Emulator (for v2)
* Start


**Build status**

![Build Status](https://sfa-gov-uk.visualstudio.com/_apis/public/build/definitions/c39e0c0b-7aff-4606-b160-3566f3bbce23/134/badge)


## Methods ##

    api/provider/
        GET       {providerId}/commitments
        GET       {providerId}/commitments/{commitmentId}
        GET       {providerId}/apprenticeships
        GET       {providerId}/apprenticeships/{apprenticeshipId}
        PATCH     {providerId}/commitments/{commitmentId}
        DELETE    {providerId}/commitments/{commitmentId}
        POST      {providerId}/commitments/{commitmentId}/apprenticeships
        PUT       {providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}
        POST      {providerId}/commitments/{commitmentId}/apprenticeships/bulk
        DELETE    {providerId}/apprenticeships/{apprenticeshipId}
        GET       {providerId}/relationships/{employerAccountId}/{legalEntityId}
        GET       {providerId}/relationships/{commitmentId}
        PATCH     {providerId}/relationships/{employerAccountId}/{legalEntityId}

    api/employer/
        GET       {accountId}/commitments
        GET       {accountId}/commitments/{commitmentId}
        GET       {accountId}/apprenticeships
        GET       {accountId}/apprenticeships/{apprenticeshipId}
        POST      {accountId}/commitments/
        PATCH     {accountId}/commitments/{commitmentId}
        DELETE    {accountId}/commitments/{commitmentId}
        POST      {accountId}/commitments/{commitmentId}/apprenticeships
        PUT       {accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}
        PATCH     {accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}
        DELETE    {accountId}/apprenticeships/{apprenticeshipId}

## Client ##

A .Net client for the Api can be found on [NuGet](https://www.nuget.org/packages/SFA.DAS.Tasks.Api.Client/).


## Configuration for running locally ##

Get configuration from das-employer-config repo das-commitments/SFA.DAS.CommitmentsV2.json
```
    change following values
        DatabaseConnectionString:                               <Your Database>
        ReadOnlyDatabaseConnectionString:                       <Your Database>
        NServiceBusConfiguration.LearningTransportFolderPath:   <Set this to the path you want events to publish to locally or leave blank to publish in solution folder>
        ReservationsClientApi.UseStub:                          <true>
        RedisConnectionString:                                  <localhost>
        AccountApi.ApiBaseUrl:                                  <If running stubs project use -> http://localhost:3999/accounts-api/>
        LevyTransferMatchingInnerApiConfiguration.BaseUrl:      <If running stubs project use -> http://localhost:3999/levy-transfer-matching-api/>
```

## Design time debugging for Entity Framework queries ##

In Commitments V2 using the LinqPad tool (https://www.linqpad.net/GetFile.aspx?LINQPad8Setup.exe) EF queries can be developed directly against a database, however the database model in Commitments V2 is configured with differences from the SQL tables. 

To make it easier to develop queries against an Azure database the ```AzureAdTokenInterceptor``` and ```ProviderCommitmentsDesignTimeDbContext``` classes have been added. 

Using LinqPad a connection can be established using the ```ProviderCommitmentsDesignTimeDbContext``` and by specifying a connection string in the format ```Server=tcp:your.database.windows.net,1433;Database=your-db;Encrypt=True```, the ```AzureAdTokenInterceptor``` will attach a windows token from the current users ```az login``` configuration.

EF queries can be written and debugged which can be directly used in Commitments V2 code.