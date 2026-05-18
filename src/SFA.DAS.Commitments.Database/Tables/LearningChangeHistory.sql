CREATE TABLE dbo.LearningChangeHistory (
	Id uniqueidentifier NOT NULL,
	[Source] tinyint NOT NULL,
	ChangeType tinyint NOT NULL,
	Description NVARCHAR(1024) NOT NULL,
	UserId uniqueidentifier NULL,
	ApprenticeshipId bigint NOT NULL,
	LearnerName NVARCHAR(100) NOT NULL,
	LearningKey uniqueidentifier NULL,
	Created datetime2(0) NOT NULL,
	AppliedDate datetime2(0) NOT NULL,
	AccountId bigint  NOT NULL,
	UKPRN bigint NOT NULL,
	ProviderName nvarchar(100) NOT NULL,
	EmployerName nvarchar(100) NOT NULL,
	CONSTRAINT LearningChangeHistory_PK PRIMARY KEY (Id)
)
GO

 CREATE NONCLUSTERED INDEX LearningChangeHistory_AccountId_IDX ON [dbo].[LearningChangeHistory] ([AccountId] ASC)  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY] 
 GO

 CREATE NONCLUSTERED INDEX LearningChangeHistory_AppliedDate_IDX ON [dbo].[LearningChangeHistory] ([AppliedDate] ASC)  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY] 
 GO

 CREATE NONCLUSTERED INDEX LearningChangeHistory_ApprenticeshipId_IDX ON [dbo].[LearningChangeHistory] ([ApprenticeshipId] ASC)  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY] 
 GO

 CREATE NONCLUSTERED INDEX LearningChangeHistory_Created_IDX ON [dbo].[LearningChangeHistory] ([Created] ASC)  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY]
 GO