$uid = "e8d34963-8a5c-4d62-8778-0d47ee0f22fa"
$pwd = $env:spipwd
$tenantId = "1a92889b-8ea1-4a16-8132-347814051567"
$secPwd = ConvertTo-SecureString $pwd -AsPlainText -Force
$credentials = New-Object System.Management.Automation.PSCredential ($uid, $secPwd)

Add-AzurermAccount -ServicePrincipal -Tenant $tenantId -Credential $credentials
#Set-AzureSubscription –SubscriptionName $env:subscription
Select-AzureSubscription -Default -SubscriptionName $env:subscription
    
$Default= Get-AzureSubscription -SubscriptionName $env:subscription
write-host $Default.IsCurrent

$StorageName = "das$env:environmentname$env:type"+"str"
$myStoreKey = (Get-AzureStorageKey –StorageAccountName $StorageName).Primary 
write-host $myStoreKey
$part1="DefaultEndpointsProtocol=https;AccountName="
$part2=";AccountKey="

$Env:StorageConnectionString= "$part1$StorageName$part2$myStoreKey"

$SourcePath = (Get-Item -Path ".\" -Verbose).FullName

$testPath = Test-Path $SourcePath

$regex = "__[A-Za-z0-9.]*__"
$patterns = @()
$matches = @()



if($testPath)
{
	Write-Output "Path Exists"
	
	$sourceDir = Get-ChildItem $SourcePath -recurse
	
	
	$List = $sourceDir | where {$_.extension -eq ".cscfg" -or $_.name -like "*.config" -or $_.name -like "*.json" -or $_.Name -like "*.csdef" -or $_.Name -like "*.publish.xml"} 
	
	
	Foreach($file in $list)
	{
		$destinationPath = $file.FullName
		$tempFile = join-path $file.DirectoryName ($file.BaseName + ".tmp")
		
		Copy-Item -Force $file.FullName $tempFile

		$matches = select-string -Path $tempFile -Pattern $regex -AllMatches | % { $_.Matches } | % { $_.Value }
		
		ForEach($match in $matches)
		{
		  $matchedItem = $match
		  $matchedItem = $matchedItem.Trim('_')
		  $matchedItem = $matchedItem -replace '\.','_'
		  (Get-Content $tempFile) | 
		  Foreach-Object {
			$_ -replace $match,(get-item env:$matchedItem).Value
		  } | 
		Set-Content $tempFile -Force
		}

		Copy-Item -Force $tempFile $DestinationPath
		Remove-Item -Force $tempFile
	}
}
else
{
	Write-Output "Path Does Not Exist"
}