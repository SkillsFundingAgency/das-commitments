CREATE PROCEDURE [dbo].[CreateMessage]
	@CommitmentId BIGINT,
	@Author NVARCHAR(255),
	@Text NVARCHAR(MAX),
	@CreatedBy TINYINT
AS

INSERT INTO [dbo].[Message]
	(CommitmentId, Author, [Text], CreatedBy, CreatedDateTime)
VALUES
	(@CommitmentId, @Author, @Text, @CreatedBy, GETDATE())
