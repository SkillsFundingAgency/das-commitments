﻿using System.Text;

namespace SFA.DAS.CommitmentsV2.Api.ErrorHandler;

public static class ExceptionExtensions
{
    public static string AggregateMessages(this Exception ex)
    {
        var newlines = Environment.NewLine.ToArray();
        var messageBuilder = new StringBuilder(ex.Message.Trim(newlines));

        while (ex.InnerException != null)
        {
            messageBuilder.AppendLine();
            messageBuilder.Append(ex.InnerException.Message.Trim(newlines));

            ex = ex.InnerException;
        }

        return messageBuilder.ToString();
    }
}