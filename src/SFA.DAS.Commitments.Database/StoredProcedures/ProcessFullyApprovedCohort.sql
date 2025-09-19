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
    SET PaymentStatus = 1, StartDate = StartDate
    WHERE CommitmentId = @cohortId
    
    INSERT INTO [dbo].[PriceHistory] (ApprenticeshipId, Cost, FromDate, TrainingPrice, AssessmentPrice)
    SELECT Id, Cost, StartDate, TrainingPrice, EndPointAssessmentPrice
    FROM [dbo].[Apprenticeship]
    WHERE CommitmentId = @cohortId

END