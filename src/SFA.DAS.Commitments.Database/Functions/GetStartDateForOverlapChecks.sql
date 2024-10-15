CREATE FUNCTION [dbo].[GetStartDateForOverlapChecks](@PaymentsSimplificationApprenticeship BIT, @StartDate DATETIME2, @ActualStartDate DATETIME2)
RETURNS DATETIME2
AS
BEGIN
	IF (@PaymentsSimplificationApprenticeship = 1) -- On Payments Simplification (Pilot)
	BEGIN
		RETURN @ActualStartDate
	END
	RETURN @StartDate
END
GO
