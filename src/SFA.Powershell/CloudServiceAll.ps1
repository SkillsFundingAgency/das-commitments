##Login to Subscription##
$uid = "e8d34963-8a5c-4d62-8778-0d47ee0f22fa"
$pwd = $env:spipwd
$tenantId = "1a92889b-8ea1-4a16-8132-347814051567"
$secPwd = ConvertTo-SecureString $pwd -AsPlainText -Force
$credentials = New-Object System.Management.Automation.PSCredential ($uid, $secPwd)

Add-AzurermAccount -ServicePrincipal -Tenant $tenantId -Credential $credentials
Select-AzureSubscription -Default -SubscriptionName $env:subscription
    
$Default= Get-AzureSubscription -SubscriptionName $env:subscription
write-host $Default.SubscriptionName 
write-host $Default.IsCurrent
 
 If ($Default.IsCurrent -eq 'True')
 {

 $CloudServiceArray= @("das-$env:environmentname-task-cs","das-$env:environmentname-comt-cs","das-$env:environmentname-pas-cs")
 
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
    #New-AzureService -ServiceName "$cloudservice1" -Label "$Cloudservice1" -Location "North Europe" 
    }
    }

    }
    else
    {
    write-host "Subscription is not set"
    }
