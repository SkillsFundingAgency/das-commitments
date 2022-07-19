CREATE VIEW [dbo].[ApprenticeshipsCreated]
	AS SELECT Id, CreatedOn,
		CASE 
			WHEN DeliveryModel IS NULL THEN 0
			ELSE DeliveryModel
		END AS DeliveryModel
	FROM [Apprenticeship]
