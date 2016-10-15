#Move Storage to correct Resource Group

$secpasswd = ConvertTo-SecureString "$env:spipwd" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("e8d34963-8a5c-4d62-8778-0d47ee0f22fa",$secpasswd)
Login-AzureRmAccount -ServicePrincipal -Tenant 1a92889b-8ea1-4a16-8132-347814051567 -Credential $mycreds

$Storagetask= 'taskstr'
$Storagecomt='comtstr'
$Storagepas='passtr'

$task= "das-$env:enviroment-task-rg"
$comt="das-$env:enviroment-comt-rg"
$pas="das-$env:enviroment-pas-rg"


$find = find-AzureRmResource -ResourceGroupNameContains "Default-Storage-NorthEurope" 

    foreach ($result2 in $find)
{
    if ($result2.ResourceName -like "*$env:enviroment$storagetask") 

{   
    write-host -ForegroundColor Yellow "Moving Storage Resource" $result2.ResourceId
    Move-AzureRmResource -DestinationResourceGroupName $task -ResourceId $result2.ResourceId -Force

}

    elseif ($result2.ResourceName -like "*$env:enviroment$storagepas") 
{
    write-host -ForegroundColor Yellow "Moving Storage Resource" $result2.ResourceId
    Move-AzureRmResource -DestinationResourceGroupName $pas -ResourceId $result2.ResourceId -Force
}

    elseif ($result2.ResourceName -like "*$env:enviroment$storagecomt") 
{
    write-host -ForegroundColor Yellow "Moving Storage Resource"
    Move-AzureRmResource -DestinationResourceGroupName $comt -ResourceId $result2.ResourceId -Force

}
}
