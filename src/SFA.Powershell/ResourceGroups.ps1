

$ResourceGroupName = "das-$env:environmentname-$env:type-rg"

$Location= "North Europe"


##Login to Subscription##
$uid = "e8d34963-8a5c-4d62-8778-0d47ee0f22fa"
$pwd = $env:spipwd
$tenantId = "1a92889b-8ea1-4a16-8132-347814051567"
$secPwd = ConvertTo-SecureString $pwd -AsPlainText -Force
$credentials = New-Object System.Management.Automation.PSCredential ($uid, $secPwd)

Add-AzurermAccount -ServicePrincipal -Tenant $tenantId -Credential $credentials
Select-AzureSubscription -Default -SubscriptionName $env:subscription
    
$Default= Get-AzureSubscription -SubscriptionName $env:subscription
write-host $Default.IsCurrent

If($Default.IsCurrent -eq 'True'){

Write-Host "Checking if Resource Group '$ResourceGroupName' exists in subscription '$default.SubscriptionName' "
             
$service =  Get-AzureRmResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
write-host $service.ProvisioningState

if($service)
{
	Write-Host -ForegroundColor Yellow "Resource Group Already Exists";

}
else
{
   
    Write-Host "Creating Resource Group"
    New-AzureRmResourceGroup -Location $location -Name $ResourceGroupName

}

}