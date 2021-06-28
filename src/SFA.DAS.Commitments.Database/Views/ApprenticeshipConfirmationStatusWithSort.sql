CREATE VIEW [dbo].[ApprenticeshipConfirmationStatusWithSort]
AS 
SELECT 
	[Id], 
	[ApprenticeshipId],
	[ApprenticeshipConfirmedOn],
	[CommitmentsApprovedOn],
	[ConfirmationOverdueOn],
	CASE 
		WHEN CommitmentsApprovedOn IS NULL THEN NULL
		WHEN ConfirmationOverdueOn < GETDATE() THEN 'O'
		WHEN ApprenticeshipConfirmedOn IS NOT NULL THEN 'C'
		ELSE 'U'
	END AS ConfirmationStatusSort
FROM [dbo].[ApprenticeshipConfirmationStatus]
GO