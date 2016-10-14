 $secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds
 
 $CloudServiceArray= @("das-$env:enviroment-task-cs","das-$env:enviroment-comt-cs","das-$env:enviroment-pas-cs")
 
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
    
