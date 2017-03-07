# Commitments API #

Commitments API for the Digital Apprenticeship Service

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
