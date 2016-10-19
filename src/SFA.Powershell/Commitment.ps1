﻿#One Click Enviroment Build for Commitments including Config DB

#SPI Details

$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

#Static Variables

$Storagetask= 'taskstr'
$Storagecomt='comtstr'
$Storagepas='passtr'
$task= "das-$env:enviroment-task-rg"
$comt="das-$env:enviroment-comt-rg"
$pas="das-$env:enviroment-pas-rg"

#Arrays

$ResourceGroupArray = @("$task","$comt","$pas")
$CloudServiceArray= @("das-$env:enviroment-task-cs","das-$env:enviroment-comt-cs","das-$env:enviroment-pas-cs")
$StorageArray= @("das$env:enviroment$StorageTask","das$env:enviroment$StorageComt","das$env:enviroment$Storagepas")

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


#Resource Group Creation Array
foreach ($result in $ResourceGroupArray){
Get-AzureRmResourceGroup -Name $result -ev notPresent -ea 0
if ($notPresent)
{
    Write-Host -ForegroundColor Yellow "Creating Resource Group $result"
    New-AzureRmResourceGroup -Name $result -Location "North Europe" -Force
}
else
{
    Write-Host -ForegroundColor Yellow "Group Exisits $result"
}

}

#Cloud Service Build
 foreach ($CloudService1 in $Cloudservicearray)
    {
    $details= @(Get-AzureService -ServiceName $CloudService1 -ErrorAction SilentlyContinue)
    If ($details.count -eq 1)
    {
    write-host "Cloud Service Already exist"$details.Servicename
    }
    else 
    {
    Write-Host "Creating $cloudservice1 Service"
    New-AzureService -ServiceName "$cloudservice1" -Label "$Cloudservice1"-Location "North Europe" 
    }
    }


#Storage Build

    
foreach ($result2 in $StorageArray){
$details =@(Get-AzureStorageAccount -StorageAccountName $result2 -ErrorAction SilentlyContinue)

if ($details.count -eq 1)
{
    Write-Host -ForegroundColor Yellow "Storage Group $Result2 already deployed"
   
    
}
else
{
     Write-Host -ForegroundColor Yellow "Creating $result2 Storage"
    New-AzureStorageAccount -StorageAccountName $result2 -label $result2 -Location "North Europe" 
    Start-Sleep -Seconds 120
}
}

#Move Resources
#Storage


   $find = find-AzureRmResource -ResourceGroupNameContains "Default-Storage-NorthEurope" 

    foreach ($result2 in $find)
{
    if ($result2.ResourceName -like "*$env:enviroment$storagetask") 

{   
    write-host -ForegroundColor Yellow "Moving Storage Resource" $result2.ResourceId
    Move-AzureRmResource -DestinationResourceGroupName $task -ResourceId $result2.ResourceId -Force

}

    elseif ($result2.ResourceName -like "*$env:enviroment$storagepas") 
{
    write-host -ForegroundColor Yellow "Moving Storage Resource" $result2.ResourceId
    Move-AzureRmResource -DestinationResourceGroupName $pas -ResourceId $result2.ResourceId -Force
}

    elseif ($result2.ResourceName -like "*$env:enviroment$storagecomt") 
{
    write-host -ForegroundColor Yellow "Moving Storage Resource"
    Move-AzureRmResource -DestinationResourceGroupName $comt -ResourceId $result2.ResourceId -Force

}
}



#Cloud Services

 foreach ($CloudService1 in $Cloudservicearray)
    {
    $details= find-AzureRmResource -ResourceGroupNameContains $CloudService1
    
     if ($details.ResourceName -like "*$env:enviroment-pas*"){
    write-host -ForegroundColor Yellow "Moving Cloud Service $Details "
    Move-AzureRmResource -DestinationResourceGroupName $pas  -ResourceId $details.ResourceId -force
     }
     elseif ($details.ResourceName -like "*$env:enviroment-task*"){
    write-host -ForegroundColor Yellow "Moving Cloud Service $Details "
    Move-AzureRmResource -DestinationResourceGroupName $task  -ResourceId $details.ResourceId -force
     }
    elseif ($details.ResourceName -like "*$env:enviroment-comt*"){
    write-host -ForegroundColor Yellow "Moving Cloud Service $Details "
    Move-AzureRmResource -DestinationResourceGroupName $comt  -ResourceId $details.ResourceId -force
     }
     }


#Delete the created Cloud Service RG
foreach ($CloudService1 in $Cloudservicearray)
    {
    $details= Get-AzureRmResourceGroup -Name $CloudService1
   
    if ($details.ResourceGroupName -like "*$env:enviroment-pas*")
    {
      write-host -ForegroundColor Yellow "Deleting Cloud Service" $Details.Resourcegroupname
      Remove-AzureRmResourceGroup -Name "$cloudservice1" -Force
     } 
     elseif ($details.ResourceGroupName -like "*$env:enviroment-comt*") 
     {
     write-host -ForegroundColor Yellow "Deleteing Cloud Service" $Details.Resourcegroupname
      Remove-AzureRmResourceGroup -Name "$cloudservice1" -Force
     }
     elseif ($details.ResourceGroupName -like "*$env:enviroment-task*") 
     {
     write-host -ForegroundColor Yellow "Deleting Cloud Service" $Details.Resourcegroupname
       Remove-AzureRmResourceGroup -Name "$cloudservice1" -Force
     }
     else { write-host -ForegroundColor Yellow "Resource Group isnt available"}
     }

#Delete the Default Storage Area
    $details= @(find-AzureRmResource -ResourceGroupNameContains Default-Storage-NorthEurope )

    If ($details.Count -ge 1)
    {
    write-host -ForegroundColor Red "Cant Delete Resource Group Default-Storage-NorthEurope"
     
     }
    elseif ([string]::IsNullorEmpty($details))
    {
     write-host -ForegroundColor Yellow "Resource Group is being removed"
     Remove-AzureRmResourceGroup -Name Default-Storage-NorthEurope -Force -ErrorAction SilentlyContinue
    }
  

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
