CREATE PROCEDURE [dbo].[CreateApprenticeshipUpdate]
(
		@ApprenticeshipId BIGINT,
		@Originator TINYINT,
		@FirstName NVARCHAR(100) NULL, 
		@LastName NVARCHAR(100) NULL, 
		@TrainingType INT NULL, 
		@TrainingCode NVARCHAR(20) NULL, 
		@TrainingName NVARCHAR(126) NULL, 
		@Cost DECIMAL NULL, 
		@StartDate DATETIME NULL, 
		@EndDate DATETIME NULL, 
		@DateOfBirth DATETIME NULL
)
AS

	INSERT INTO [dbo].[ApprenticeshipUpdate]
	(
		[ApprenticeshipId],
		[Originator] ,
		[FirstName], 
		[LastName], 
		[TrainingType], 
		[TrainingCode], 
		[TrainingName], 
		[Cost], 
		[StartDate], 
		[EndDate], 
		[DateOfBirth]
	)
	values
	(
		@ApprenticeshipId,
		@Originator,
		@FirstName, 
		@LastName, 
		@TrainingType, 
		@TrainingCode, 
		@TrainingName, 
		@Cost, 
		@StartDate, 
		@EndDate, 
		@DateOfBirth
	)