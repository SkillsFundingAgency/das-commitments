#Login

$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds


#SQL Variables

$sqlServerVersion = "12.0"
$sqlServerLocation = "North Europe"
$databaseEdition = "Standard"
$databaseServiceLevel = "S0"

#Task DB

#$serverPassword = $env:SQLServerPasswordtaskdb
$serverAdmintask = "sqlt4sk4dm"
$securePassword = ConvertTo-SecureString "$env:SQLServerPasswordtaskdb" -AsPlainText -Force
$serverCreds = New-Object System.Management.Automation.PSCredential ($serverAdmintask, $securePassword)
$sqlServerName = "das-$env:enviroment-task-sql"
$databaseName = "das-$env:enviroment-task-db"


#Comt DB

#$serverPasswordcomt = $env:SQLServerPasswordcomtdb
$serverAdmincomt = "sqlcomtadm"
$securePasswordcomt = ConvertTo-SecureString "$env:SQLServerPasswordcomtdb" -AsPlainText -Force
$serverCredscomt = New-Object System.Management.Automation.PSCredential ($serverAdmincomt, $securePasswordcomt)
$sqlServerNamecomt = "das-$env:enviroment-comt-sql"
$databaseNamecomt = "das-$env:enviroment-comt-db"



#Azure SQL Server
#Task SQL Server & Database
Get-AzureRmSqlDatabase -ResourceGroupName das-$env:enviroment-task-rg -ServerName das-$env:enviroment-task-sql -ev notPresent -ea 0
$resourcegroupName= "das-$env:enviroment-task-rg"
if ($notPresent)
{
  write-host -ForegroundColor Yellow  "Creating Azure SQL Server das-$env:enviroment-task-sql"
    $sqlServer = New-AzureRmSqlServer -ServerName $sqlServerName -SqlAdministratorCredentials $ServerCreds `
    -Location $sqlServerLocation -ResourceGroupName $resourceGroupName -ServerVersion $sqlServerVersion

    $currentDatabase = New-AzureRmSqlDatabase -ResourceGroupName $resourceGroupName `
     -ServerName $sqlServerName -DatabaseName $databaseName `
     -Edition $databaseEdition -RequestedServiceObjectiveName $databaseServiceLevel
}
else
{
  write-host -ForegroundColor Yellow  "Azure SQL Server das-$env:enviroment-task-sql already deployed "
}


#Task DB
Get-AzureRmSqlDatabase -ResourceGroupName das-$env:enviroment-task-rg -ServerName das-$env:enviroment-task-sql -DatabaseName das-$env:enviroment-task-db  -ev notPresent -ea 0
$resourcegroupName= "das-$env:enviroment-task-rg"
if ($notPresent)
{
  write-host -ForegroundColor Yellow  "Creating Azure SQL Server Database das-$env:enviroment-task-db"
    
$currentDatabase = New-AzureRmSqlDatabase -ResourceGroupName $resourceGroupName `
 -ServerName $sqlServerName -DatabaseName $databaseName `
 -Edition $databaseEdition -RequestedServiceObjectiveName $databaseServiceLevel

}
else
{
  write-host -ForegroundColor Yellow  "Azure SQL Database das-$env:enviroment-task-db already deployed"
}


#Azure Azure SQL Server Comt

Get-AzureRmSqlDatabase -ResourceGroupName das-$env:enviroment-comt-rg -ServerName das-$env:enviroment-comt-sql -ev notPresent -ea 0
$resourcegroupName= "das-$env:enviroment-comt-rg"

if ($notPresent)
{
  write-host -ForegroundColor Yellow  "Creating Azure SQL Server das-$env:enviroment-comt-sql"
    
    $sqlServer = New-AzureRmSqlServer -ServerName $sqlServerNamecomt -SqlAdministratorCredentials $ServerCredscomt -Location $sqlServerLocation -ResourceGroupName $resourceGroupName -ServerVersion $sqlServerVersion

    $currentDatabase = New-AzureRmSqlDatabase -ResourceGroupName $resourceGroupName `
    -ServerName $sqlServerNamecomt -DatabaseName $databaseNamecomt `
    -Edition $databaseEdition -RequestedServiceObjectiveName $databaseServiceLevel
}
else
{
  write-host -ForegroundColor Yellow  "Azure SQL Server das-$env:enviroment-comt-sql already deployed"
}

Get-AzureRmSqlDatabase -ResourceGroupName das-$env:enviroment-comt-rg -ServerName das-$env:enviroment-comt-sql -DatabaseName das-$env:enviroment-comt-db -ev notPresent -ea 0
$resourcegroupName= "das-$env:enviroment-comt-rg"

if ($notPresent)
{
  write-host -ForegroundColor Yellow  "Creating Azure SQL Server Database das-$env:enviroment-comt-db"
    
    $currentDatabase = New-AzureRmSqlDatabase -ResourceGroupName $resourcegroupName `
    -ServerName $sqlServerNamecomt -DatabaseName $databaseNamecomt `
    -Edition $databaseEdition -RequestedServiceObjectiveName $databaseServiceLevel

}
else
{
  write-host -ForegroundColor Yellow  "Azure SQL Database das-$env:enviroment-comt-db already deployed"
}
