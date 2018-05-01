CREATE PROCEDURE [dbo].[UpdateApprenticeshipUpdateStatus]
	@id BIGINT,
	@status TINYINT
AS
	SET XACT_ABORT ON

	BEGIN TRAN
		-- assumes Status != pending

		-- different ways of doing the same thing
		-- but they all have the same actual execution plan

		--UPDATE [dbo].[Apprenticeship]
		--SET [PendingOriginator] = NULL
		--FROM [dbo].[Apprenticeship] AS A
		--INNER JOIN [dbo].[ApprenticeshipUpdate] AS AU
		--	ON A.Id = AU.ApprenticeshipId
		--WHERE
		--	AU.Id = @id;

		--UPDATE [dbo].[Apprenticeship]
		--SET [PendingOriginator] = NULL
		--FROM [dbo].[Apprenticeship] AS A
		--INNER JOIN [dbo].[ApprenticeshipUpdate] AS AU
		--	ON A.Id = AU.ApprenticeshipId
		--	AND AU.Id = @id;

		UPDATE [dbo].[Apprenticeship]
		SET [PendingOriginator] = NULL
		WHERE Id=(SELECT ApprenticeshipId from ApprenticeshipUpdate where Id = @Id)

		UPDATE [dbo].[ApprenticeshipUpdate]
		SET Status = @status
	    WHERE Id = @id;
	COMMIT