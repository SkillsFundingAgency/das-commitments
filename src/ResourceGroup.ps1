#Resource Group Creation

#SPI Details

$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

$task= "das-$env:enviroment-task-rg"
$comt="das-$env:enviroment-comt-rg"
$pas="das-$env:enviroment-pas-rg"

#Arrays
$ResourceGroupArray = @("$task","$comt","$pas")

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