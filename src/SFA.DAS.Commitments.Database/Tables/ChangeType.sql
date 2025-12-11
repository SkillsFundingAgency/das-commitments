CREATE TABLE [dbo].[ChangeType]             -- Richard - I'm not sure how this table is going to be populated, or what the values of ChangeTypes are going to be
(
    [Id] [tinyint] NOT NULL,
	[Description] [nvarchar](100) NOT NULL,
    CONSTRAINT [PK_ChangeType] PRIMARY KEY CLUSTERED ([Id] ASC),
)
