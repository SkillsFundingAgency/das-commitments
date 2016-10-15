
$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

$task= "das-prefix-task-rg"
$comt="das-prefix-comt-rg"
$pas="das-prefix-pas-rg"

$CloudServiceArray= @("das-prefix-task-cs","das-prefix-comt-cs","das-prefix-pas-cs")

 foreach ($CloudService1 in $Cloudservicearray)
    {
    $details= find-AzureRmResource -ResourceGroupNameContains $CloudService1
    
     if ($details.ResourceName -like "*prefix-pas-cs"){
    write-host -ForegroundColor Yellow "Moving Cloud Service" $Details.ResourceName "to $pas"
    Move-AzureRmResource -DestinationResourceGroupName $pas -ResourceId $details.ResourceId -force
     }
     elseif ($details.ResourceName -like "*prefix-task-cs"){
    write-host -ForegroundColor Yellow "Moving Cloud Service" $Details.ResourceName "to $task"
    Move-AzureRmResource -DestinationResourceGroupName $task -ResourceId $details.ResourceId -force
     }
    elseif ($details.ResourceName -like "*prefix-comt-cs"){
    write-host -ForegroundColor Yellow "Moving Cloud Service" $Details.ResourceName "to $comt"
    Move-AzureRmResource -DestinationResourceGroupName $comt -ResourceId $details.ResourceId -force
     }
     }
