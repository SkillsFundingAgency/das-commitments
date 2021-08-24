CREATE PROCEDURE [dbo].[ImportStandardOptions](
    @standardOptions [dbo].[StandardOptions] READONLY)
AS
BEGIN    
    INSERT INTO [dbo].[StandardOption]
    SELECT * FROM @standardOptions
END
    
