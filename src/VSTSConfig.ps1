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