CREATE VIEW [dbo].[FlexibleApprenticeships]
	AS SELECT
		  Id
		, CreatedOn
		, DeliveryModel
		, EmploymentPrice
		, EmploymentEndDate
	FROM [ApprenticeshipsCreated]
	LEFT JOIN [ApprenticeshipFlexibleEmployment] ON [ApprenticeshipsCreated].Id = [ApprenticeshipFlexibleEmployment].ApprenticeshipId