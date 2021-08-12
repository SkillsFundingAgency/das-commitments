CREATE FUNCTION [dbo].[GetEndDateForOverlapChecks](@PaymentStatus SMALLINT, @ExpectedEndDate DATETIME2, @StopDate DATETIME2, @CompetionDate DATETIME2)
RETURNS DATETIME2
AS
BEGIN
	IF (@PaymentStatus = 3) -- Withdrawn
	BEGIN
		RETURN @StopDate
	END

	IF (@PaymentStatus = 4 /* Completed */ AND @CompetionDate <= @ExpectedEndDate)
	BEGIN
		RETURN @CompetionDate
	END

	RETURN @ExpectedEndDate
END
GO
