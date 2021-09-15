/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

EXEC sp_refreshview [dbo.CommitmentSummaryWithMessages]

/* Reporter Role Access */

IF DATABASE_PRINCIPAL_ID('DashboardReporting') IS NULL
BEGIN
    CREATE ROLE [DashboardReporting]
END

GRANT SELECT ON SCHEMA :: DashboardReporting TO DashboardReporting