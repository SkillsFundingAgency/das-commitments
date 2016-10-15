
$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

$CloudServiceArray= @("das-$env:enviroment-task-cs","das-$env:enviroment-comt-cs","das-$env:enviroment-pas-cs")

 foreach ($CloudService1 in $Cloudservicearray)
    {
    $details= find-AzureRmResource -ResourceGroupNameContains $CloudService1
    
     if ($details.ResourceName -like "*$env:enviroment-pas*"){
    write-host -ForegroundColor Yellow "Moving Cloud Service $Details "
    Move-AzureRmResource -DestinationResourceGroupName $pas  -ResourceId $details.ResourceId -force
     }
     elseif ($details.ResourceName -like "*$env:enviroment-task*"){
    write-host -ForegroundColor Yellow "Moving Cloud Service $Details "
    Move-AzureRmResource -DestinationResourceGroupName $task  -ResourceId $details.ResourceId -force
     }
    elseif ($details.ResourceName -like "*$env:enviroment-comt*"){
    write-host -ForegroundColor Yellow "Moving Cloud Service $Details "
    Move-AzureRmResource -DestinationResourceGroupName $comt  -ResourceId $details.ResourceId -force
     }
     }
