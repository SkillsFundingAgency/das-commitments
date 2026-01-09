CREATE PROCEDURE [dbo].[ProcessFullyApprovedCohort]
    @cohortId BIGINT,
    @accountId BIGINT,
    @apprenticeshipEmployerType INT
AS
BEGIN
    -- Check if already processed to make this idempotent
    IF EXISTS (
        SELECT 1 
        FROM [dbo].[Commitment] 
        WHERE Id = @cohortId AND IsFullApprovalProcessed = 1
    )
    BEGIN
        -- Already processed, return early (idempotent)
        RETURN;
    END
    
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