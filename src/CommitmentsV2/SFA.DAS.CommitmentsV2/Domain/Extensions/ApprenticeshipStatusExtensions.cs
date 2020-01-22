﻿using System;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class ApprenticeshipStatusExtensions
    {
        public static ApprenticeshipStatus MapApprenticeshipStatus(this Apprenticeship source, ICurrentDateTime currentDateTime)
        {
            var now = new DateTime(currentDateTime.UtcNow.Year, currentDateTime.UtcNow.Month, 1);
            var waitingToStart = source.StartDate.HasValue && source.StartDate.Value > now;

            switch (source.PaymentStatus)
            {
                case PaymentStatus.Active:
                    return waitingToStart ? ApprenticeshipStatus.WaitingToStart : ApprenticeshipStatus.Live;
                case PaymentStatus.Paused:
                    return ApprenticeshipStatus.Paused;
                case PaymentStatus.Withdrawn:
                    return ApprenticeshipStatus.Stopped;
                case PaymentStatus.Completed:
                    return ApprenticeshipStatus.Completed;
                case PaymentStatus.Deleted:
                    return ApprenticeshipStatus.Live;
                default:
                    return ApprenticeshipStatus.WaitingToStart;
            }
        }
    }
}