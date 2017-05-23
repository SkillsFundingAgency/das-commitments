CREATE TABLE [dbo].[CustomProviderPaymentPriority]
(
	[EmployerAccountId] BIGINT NOT NULL , 
    [ProviderId] BIGINT NOT NULL, 
    [PriorityOrder] INT NOT NULL, 
    PRIMARY KEY ([EmployerAccountId], [ProviderId])
)
