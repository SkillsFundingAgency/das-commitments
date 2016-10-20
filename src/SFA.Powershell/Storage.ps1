$Location= "North Europe"

$ResourceGroupName = "das-$env:environmentname-$env:type-rg"

$StorageName = "das$env:environmentname$env:type"+"str"



##Login to Subscription##
$uid = "e8d34963-8a5c-4d62-8778-0d47ee0f22fa"
$pwd = $env:spipwd
$tenantId = "1a92889b-8ea1-4a16-8132-347814051567"
$secPwd = ConvertTo-SecureString $pwd -AsPlainText -Force
$credentials = New-Object System.Management.Automation.PSCredential ($uid, $secPwd)

Add-AzurermAccount -ServicePrincipal -Tenant $tenantId -Credential $credentials
#Set-AzureSubscription –SubscriptionName $env:subscription
Select-AzureSubscription -Default -SubscriptionName $env:subscription
    
$Default= Get-AzureSubscription -SubscriptionName $env:subscription
write-host $Default.IsCurrent

If($Default.IsCurrent -eq 'True'){

Write-Host "Preparing storage '$StorageName'"
             
$service =  Get-AzureStorageAccount -StorageAccountName $StorageName -ErrorAction SilentlyContinue


if($service)
{
	Write-Host -ForegroundColor Yellow "Storage Already Exists'$ServiceName'";

}

else
{
   
    Write-Host "No storage exists building..."

    New-AzureStorageAccount -Location $Location -StorageAccountName $StorageName
    
    Write-Host "Waiting for storage to become available..."
    
    Start-sleep -s 30

    Write-Host "Moving '$StorageName' to resource group '$ResourceGroupName'..."

    $built= Get-AzureRmResource -ResourceGroupName Default-Storage-NorthEurope -ResourceName $storagename -ResourceType Microsoft.ClassicStorage/storageAccounts -ErrorAction SilentlyContinue
    write-host $built.ResourceName
    
    Move-AzureRmResource -DestinationResourceGroupName Das-demo-comt-rg -ResourceId $built.ResourceId -Force
    
}

Write-Host "[service online]" -ForegroundColor Green

}
else 
{
write-host "Not in Correct Subscription"
}



