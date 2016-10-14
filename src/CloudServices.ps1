$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds
 
 $CloudServiceArray= @("das-daniel10-task-cs","das-daniel10-comt-cs")
 $detailarray= @(Get-AzureService)

    foreach ($CloudService1 in $Cloudservicearray)
    {
  
    foreach ($name in $detailarray){
    If ($name.ServiceName -eq $CloudService1)
    {
    write-host $name.Servicename "Exists"
  
    }
   
    }
     if ($name.ServiceName -ne $CloudService1){


   write-host "Building" $CloudService1
   New-AzureService -ServiceName $CloudService1 -Label $CloudService1 -Location "North Europe"
  
   }
   }
