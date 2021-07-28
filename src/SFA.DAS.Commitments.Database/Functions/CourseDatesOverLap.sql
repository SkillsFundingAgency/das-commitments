CREATE FUNCTION [dbo].[CourseDatesOverlap](@StartDate DATETIME2, @EndDate DATETIME2, @NewStartDate DATETIME2, @NewEndDate DATETIME2)
RETURNS INT
AS
BEGIN
	DECLARE @Start INT = YEAR(@StartDate) * 100 + MONTH(@StartDate)
	DECLARE @End INT = YEAR(@EndDate) * 100 + MONTH(@EndDate)
	DECLARE @NewStart INT = YEAR(@NewStartDate) * 100 + MONTH(@NewStartDate)
	DECLARE @NewEnd INT = YEAR(@NewEndDate) * 100 + MONTH(@NewEndDate)

	IF (@NewStart <= @Start AND @NewEnd <= @Start)
	BEGIN
		RETURN 0
	END

	IF (@NewStart >= @End AND @NewEnd >= @End)
	BEGIN
		RETURN 0
	END

	RETURN 1
END
GO