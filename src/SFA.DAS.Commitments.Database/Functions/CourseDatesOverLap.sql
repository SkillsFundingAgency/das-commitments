CREATE FUNCTION [dbo].[CourseDatesOverlap](@StartDate DATETIME2, @EndDate DATETIME2, @NewStartDate DATETIME2, @NewEndDate DATETIME2)
RETURNS SMALLINT
AS
BEGIN
	DECLARE @Start INT = YEAR(@StartDate) * 100 + MONTH(@StartDate)
	DECLARE @End INT = YEAR(@EndDate) * 100 + MONTH(@EndDate)
	DECLARE @NewStart INT = YEAR(@NewStartDate) * 100 + MONTH(@NewStartDate)
	DECLARE @NewEnd INT = YEAR(@NewEndDate) * 100 + MONTH(@NewEndDate)
	DECLARE @RetVal SMALLINT
	DECLARE @StartOverlaps BIT = 0
	DECLARE @EndOverlaps BIT = 0

	IF (@NewStart <= @Start AND @NewEnd <= @Start)
	BEGIN
		RETURN 0
	END

	IF (@NewStart >= @End AND @NewEnd >= @End)
	BEGIN
		RETURN 0
	END

	IF(@NewStart > @Start AND @NewStart < @End)
	BEGIN
		SET @StartOverlaps = 1
	END

	IF(@NewEnd > @Start AND @NewEnd < @End)
	BEGIN
		SET @EndOverlaps = 1
	END

	IF(@StartOverlaps = 1 AND @EndOverlaps = 1)
	BEGIN
		SET @RetVal = 4
	END 
	ELSE IF (@StartOverlaps = 1) 
	BEGIN
		SET @RetVal = 1
	END 
	ELSE IF (@EndOverlaps = 1) 
	BEGIN
		SET @RetVal = 2
	END 

	IF @RetVal IS NULL
	BEGIN
		SET @RetVal = 3
	END 

	RETURN @RetVal
END
GO