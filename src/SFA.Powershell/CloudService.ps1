$ServiceName= "das-$env:environmentname-$env:type-cs"

$Location= "North Europe"

$ResourceGroupName = "das-$env:environmentname-$env:type-rg"

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

Write-Host "Preparing cloud service '$ServiceName' in resource group '$ResourceGroupName' in '$Location'..."
             
$service =  Get-AzureService -ServiceName "$ServiceName" -ErrorAction SilentlyContinue
write-host $service.Label

if($service)
{
	Write-Host -ForegroundColor Yellow "Service Already Exists'$ServiceName'";

}
else
{
   
    Write-Host "No service exists, creating new..."

    New-AzureService -ServiceName $ServiceName -Location "$Location"
    
    Write-Host "Looking for the new cloud service..."

    Start-sleep -s 120
    
    $built= Get-AzureRmResource -ResourceGroupName $ServiceName -ResourceName $ServiceName  -ErrorAction SilentlyContinue
    write-host $built.ResourceName
     write-host $built.ResourceId
    
    Move-AzureRmResource -DestinationResourceGroupName $ResourceGroupName -ResourceId $built.ResourceId -Force
   
    Start-sleep -s 25
    
    Write-Host "Removing resoure group '$ServiceName'..."
    Remove-AzureRmResourceGroup -Name "$ServiceName" -Force
}

Write-Host "[service online]" -ForegroundColor Green

}
else 
{
write-host "Not in Correct Subscription"
}

