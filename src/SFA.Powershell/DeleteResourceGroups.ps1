$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

$ResourceGroupDetails = Get-AzureRmResourceGroup 
foreach($RGDetail in $ResourceGroupDetails) 
{
    $count = ((Find-AzureRmResource -ResourceGroupNameContains $RGDetail.ResourceGroupName).Name).count 
    if($count -eq 0) 
    { 
        Write-Output "Deleting", $RGDetail 
        Remove-AzureRmResourceGroup -Name $RGDetail.ResourceGroupName -Force
    } 
}