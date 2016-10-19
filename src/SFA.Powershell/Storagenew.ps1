param(
[Parameter(Mandatory=$False)]
 [string]
 $EnvironmentName = "testdan",

 [Parameter(Mandatory=$False)]
 [string]
 $ServiceName= "dasdan123comtstr",
 
 [Parameter(Mandatory=$False)]
 [string]
 $Location= "North Europe",
 
 [Parameter(Mandatory=$False)]
 [string]
 $ResourceGroupName = "das-demo-comt-rg"
)

#Login
#$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
#$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
#Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds


Write-Host "Preparing cloud service '$ServiceName' in resource group '$ResourceGroupName' in '$Location'..."

Select-AzureSubscription -SubscriptionName SFA-DAS-Comt-Dev

Function WaitForService {
    Param(
        [string]$ResourceGroupName,
        
        [int]$Retries = 10
    )

    $tried = 0;
    
    while($tried -le $Retries)
    {
        try
        {
            $cloudService = Get-AzurermResource -resourcename "dasdan123comtstr" -ResourceGroupName "Default-Storage-NorthEurope"
            Write-Host "[Storage ready]" -ForegroundColor Green
            return $cloudService
        }
        catch
        {
            Write-Host "[Storage not ready yet]" -ForegroundColor Red
            Start-Sleep 5
        }
    }
}

$service =  Get-AzureStorageAccount -StorageAccountName $serviceName -ErrorAction SilentlyContinue
if($service)
{
	Write-Host "Using existing service '$ServiceName'";

}
else
{
    
    Write-Host "No storage exists, creating new..."
   Set-AzureRmContext -Confirm -SubscriptionName SFA-DAS-Comt-Dev

   New-AzureStorageAccount -Location "North Europe" -StorageAccountName $ServiceName
    
    Write-Host "Looking for the new storage..."
    
    $cloudService = WaitForService($ServiceName, 50); 
    $cloudServiceId = $cloudService.ResourceId;
    
    Write-Host "Moving '$ServiceName' ($cloudServiceId) to resource group '$ResourceGroupName'..."
    Move-AzureRmResource -DestinationResourceGroupName "$ResourceGroupName" -ResourceId $cloudServiceID -Force
    

}

Write-Host "[service online]" -ForegroundColor Green