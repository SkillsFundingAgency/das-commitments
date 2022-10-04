CREATE PROCEDURE [dbo].[ProcessFullyApprovedCohort]
    @cohortId BIGINT,
    @accountId BIGINT,
    @apprenticeshipEmployerType INT
AS
BEGIN
    UPDATE [dbo].[Commitment]
    SET ApprenticeshipEmployerTypeOnApproval = @apprenticeshipEmployerType, IsFullApprovalProcessed = 1
    WHERE Id = @cohortId
    
    UPDATE [dbo].[Apprenticeship]
    SET PaymentStatus = 1
    WHERE CommitmentId = @cohortId
    
    INSERT INTO [dbo].[PriceHistory] (ApprenticeshipId, Cost, FromDate)
    SELECT Id, Cost, ISNULL(ActualStartDate, StartDate)
    FROM [dbo].[Apprenticeship]
    WHERE CommitmentId = @cohortId

END