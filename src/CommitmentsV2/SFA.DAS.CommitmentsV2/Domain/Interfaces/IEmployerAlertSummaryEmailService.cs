﻿namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IEmployerAlertSummaryEmailService
{
    Task SendEmployerAlertSummaryNotifications();
}