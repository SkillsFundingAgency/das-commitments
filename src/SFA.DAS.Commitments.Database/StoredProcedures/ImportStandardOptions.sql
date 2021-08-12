CREATE PROCEDURE [dbo].[ImportStandardOptions](
    @standardOptions [dbo].[StandardOptions] READONLY)
AS
BEGIN
    TRUNCATE TABLE [dbo].[StandardOption]
    
    INSERT INTO [dbo].[StandardOption]
    SELECT * FROM @standardOptions
END
    
