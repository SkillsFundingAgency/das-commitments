CREATE PROCEDURE [dbo].[UpdateCustomProviderPaymentPriority]
	@EmployerAccountId BIGINT,
	@ProviderIds ProviderPriorityTable READONLY
AS

	DELETE FROM CustomProviderPaymentPriority
	WHERE EmployerAccountId = @EmployerAccountId

	INSERT INTO CustomProviderPaymentPriority
	SELECT @EmployerAccountId, p.Id, p.[Priority]
	FROM @ProviderIds p
