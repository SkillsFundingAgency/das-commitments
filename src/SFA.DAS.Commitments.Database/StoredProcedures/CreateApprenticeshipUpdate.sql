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
		@DateOfBirth DATETIME NULL,
		@CreatedOn DATETIME NULL,
		@UpdateOrigin TINYINT,
		@EffectiveFromDate DATETIME,
		@EffectiveToDate DATETIME NULL
)
AS
	-- is this enough, do we *need* to try/catch rollback?
	SET XACT_ABORT ON

	BEGIN TRAN

		UPDATE [dbo].[Apprenticeship]
		SET [PendingUpdateOriginator] = @Originator
		WHERE Id = @ApprenticeshipId

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
			[DateOfBirth],
			[CreatedOn],
			[UpdateOrigin],
			[EffectiveFromDate],
			[EffectiveToDate]
		)
		VALUES
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
			@DateOfBirth,
			@CreatedOn,
			@UpdateOrigin,
			@EffectiveFromDate,
			@EffectiveToDate
		)

		SELECT SCOPE_IDENTITY()
	COMMIT
