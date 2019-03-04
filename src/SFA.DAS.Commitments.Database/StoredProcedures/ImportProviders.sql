CREATE PROCEDURE [dbo].[ImportProviders]
    @providers [dbo].[Providers] READONLY,
    @now DATETIME
AS
    UPDATE [dbo].[Providers]
    SET [Name] = f.[Name], [Updated] = @now
    FROM @providers f
    INNER JOIN [dbo].[Providers] t ON t.[Ukprn] = f.[Ukprn]
    WHERE t.[Name] <> f.[Name]
    
    INSERT INTO [dbo].[Providers] ([Ukprn], [Name], [Created])
    SELECT f.[Ukprn], f.[Name], @now
    FROM @providers f
    LEFT JOIN [dbo].[Providers] t ON t.[Ukprn] = f.[Ukprn]
    WHERE t.[Ukprn] IS NULL