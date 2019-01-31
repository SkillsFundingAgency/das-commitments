CREATE PROCEDURE [dbo].[UpdateApprenticeshipUpdateStatus]
	@Id BIGINT,
	@Status TINYINT
AS
	SET XACT_ABORT ON

	BEGIN TRAN
		-- assumes Status != pending
		IF @Status = 0
		BEGIN;
			THROW 100000, 'State must != Pending (0)', 0
		END

		UPDATE [dbo].[Apprenticeship]
		SET [PendingUpdateOriginator] = NULL
		WHERE Id=(SELECT ApprenticeshipId from ApprenticeshipUpdate where Id = @Id)

		UPDATE [dbo].[ApprenticeshipUpdate]
		SET Status = @Status
	    WHERE Id = @Id;
	COMMIT