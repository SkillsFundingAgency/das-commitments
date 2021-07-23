CREATE VIEW [dbo].[ApprenticeshipConfirmationStatusWithSort]
AS 
SELECT 
	[ApprenticeshipId],
	[ApprenticeshipConfirmedOn],
	[CommitmentsApprovedOn],
	[ConfirmationOverdueOn],
	CASE 
		WHEN CommitmentsApprovedOn IS NULL THEN 'N'
		WHEN ApprenticeshipConfirmedOn IS NOT NULL THEN 'C'
		WHEN ConfirmationOverdueOn < GETDATE() THEN 'O'
		ELSE 'U'
	END AS ConfirmationStatusSort
FROM [dbo].[ApprenticeshipConfirmationStatus]
GO