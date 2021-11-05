CREATE TRIGGER Trg_Apprenticeship_Update
ON Apprenticeship
AFTER UPDATE 
AS
BEGIN
    UPDATE Apprenticeship
    SET UpdatedOn = GETUTCDATE()  
    FROM (
       SELECT * FROM Inserted
       EXCEPT
       SELECT * FROM Deleted) i
    WHERE i.Id = Apprenticeship.Id
END
GO