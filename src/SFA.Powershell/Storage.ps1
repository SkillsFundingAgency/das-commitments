$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

$Storagetask= 'taskstr'
$Storagecomt='comtstr'
$Storagepas='passtr'

$StorageArray= @("das$env:enviroment$StorageTask","das$env:enviroment$StorageComt","das$env:enviroment$Storagepas")
$GetStorage=@(Get-AzureStorageAccount)

 Compare-Object -ReferenceObject $GetStorage.StorageAccountName -DifferenceObject $StorageArray | 
    Where-Object { $_.SideIndicator -eq '=>' } | 
    
    ForEach-Object  { 
    try {
    #write-host $_.InputObject
    New-AzureStorageAccount -StorageAccountName $_.InputObject -label $_.InputObject -Location "North Europe" -ErrorAction Stop
    Start-Sleep -Seconds 120
    }
    catch {
    write-host $_.Exception.Message
    }
    }
