﻿using System.Linq.Expressions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;

public abstract class OrderedApprenticeshipSearchBaseService
{
    protected static async Task<ApprenticeshipSearchResult> CreatePagedApprenticeshipSearchResult(int pageNumber,
        int pageItemCount, IQueryable<Apprenticeship> apprenticeshipsQuery,
        int totalApprenticeshipsFound,
        int totalApprenticeshipsWithAlertsFound,
        int totalAvailableApprenticeships, CancellationToken cancellationToken)
    {
        List<Apprenticeship> apprenticeships;
        var selectedPageNumber = pageNumber;

        if (pageItemCount < 1 || pageNumber < 1)
        {
            apprenticeships = await apprenticeshipsQuery.ToListAsync(cancellationToken);
        }
        else
        {
            var maxPageNumber =  (int) Math.Ceiling((double)totalApprenticeshipsFound / pageItemCount);
            selectedPageNumber = pageNumber <= maxPageNumber ? pageNumber : maxPageNumber;

            apprenticeships = await apprenticeshipsQuery.Skip((selectedPageNumber - 1) * pageItemCount)
                .Take(pageItemCount)
                .ToListAsync(cancellationToken);
        }

        return new ApprenticeshipSearchResult
        {
            Apprenticeships = apprenticeships,
            TotalApprenticeshipsFound = totalApprenticeshipsFound,
            TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlertsFound,
            TotalAvailableApprenticeships = totalAvailableApprenticeships,
            PageNumber = selectedPageNumber
        };
    }

    protected static Expression<Func<Apprenticeship, object>> GetOrderByField(string fieldName)
    {
        switch (fieldName)
        {
            case nameof(Apprenticeship.FirstName):
                return apprenticeship => apprenticeship.FirstName;
            case nameof(Apprenticeship.LastName):
                return apprenticeship => apprenticeship.LastName;
            case nameof(Apprenticeship.CourseName):
                return apprenticeship => apprenticeship.CourseName;
            case nameof(Apprenticeship.Cohort.AccountLegalEntity.Name):
                return apprenticeship => apprenticeship.Cohort.AccountLegalEntity.Name;
            case nameof(Apprenticeship.StartDate):
                return apprenticeship => apprenticeship.StartDate;
            case nameof(Apprenticeship.EndDate):
                return apprenticeship => apprenticeship.EndDate;
            case nameof(Apprenticeship.Uln):
                return apprenticeship => apprenticeship.Uln;
            case nameof(Apprenticeship.ApprenticeshipConfirmationStatus.ConfirmationStatus):
                return apprenticeship => apprenticeship.ApprenticeshipConfirmationStatus.ConfirmationStatusSort;
            case "ProviderName":
                return apprenticeship => apprenticeship.Cohort.Provider.Name;
            default:
                return apprenticeship => apprenticeship.FirstName;
        }
    }

    protected static Expression<Func<Apprenticeship, object>> GetSecondarySortByField(string fieldName)
    {
        switch (fieldName)
        {
            case nameof(Apprenticeship.FirstName):
                return apprenticeship => apprenticeship.LastName;
            default:
                return GetOrderByField(fieldName);
        }
    }
}