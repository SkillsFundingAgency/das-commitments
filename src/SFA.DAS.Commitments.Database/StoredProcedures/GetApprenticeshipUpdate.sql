﻿CREATE PROCEDURE [dbo].[GetApprenticeshipUpdate]
	@ApprenticeshipId BIGINT
AS

	SELECT
	top 1
	*
	from
	ApprenticeshipUpdate
	where
	ApprenticeshipId = @ApprenticeshipId
	and [Status] = 0 --Pending