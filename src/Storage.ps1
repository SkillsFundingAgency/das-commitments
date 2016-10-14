$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

$Storagetask= 'taskstr'
$Storagecomt='comtstr'
$Storagepas='passtr'

$StorageArray= @("das$env:enviroment$StorageTask","das$env:enviroment$StorageComt","das$env:enviroment$Storagepas")


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