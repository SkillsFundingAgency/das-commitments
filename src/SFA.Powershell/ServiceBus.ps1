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


If ($env:ServiceBus -eq 'True'){
If($Default.IsCurrent -eq 'True'){

Write-Host "Preparing service bus '$ServiceName' in resource group '$ResourceGroupName' in '$Location'..."
             

            

$service =  Get-AzureSBNamespace -Name "$ServiceName" -ErrorAction SilentlyContinue

if($service)
{
	Write-Host -ForegroundColor Yellow "Service Already Exists'$ServiceName'";

}
else
{
   
    Write-Host "No service bus exists, creating new..."

    New-AzureSBNamespace -name $ServiceName -Location $Location -NamespaceType Messaging
    
    Write-Host "Looking for the new service bus..."

    Start-sleep -s 120


    
$res = Find-AzureRmResource -ResourceNameContains $ServiceName -ResourceType 'Microsoft.ServiceBus/namespaces' -ErrorAction SilentlyContinue
Move-AzureRmResource -DestinationResourceGroupName $ResourceGroupName -ResourceId $res.ResourceId


    Start-sleep -s 25
    
}

Write-Host "[service online]" -ForegroundColor Green

}
else 
{
write-host "Not in Correct Subscription"
}

}
else{

write-host "Service bus not needed for this deployment"}