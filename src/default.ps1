Import-Module .\Build\Bacon.dll -DisableNameChecking

Properties {
	$SolutionFile = "Edit.sln"
	$TargetsDir = "Target"
	$OutDir = "$TargetsDir\bin"
	$Configuration = "Debug"
	$LocalRepository = ($env:LOCALAPPDATA + "\.bacon")
	$Version = (Get-Date -Format "yyyyMMdd.HHmm.ss")
	[string[]]$Repositories = $LocalRepository, "https://packages.nuget.org/api/v2"
}

Task Default -Depends Init, Restore, Build, Publish

Task Init {
	Write-Host "Solution:`t$SolutionFile" -ForegroundColor Gray
	Write-Host "Configuration:`t$Configuration" -ForegroundColor Gray
	Write-Host "Version:`t$Version" -ForegroundColor Gray
}

Task Clean -Depends Init {
	if (Test-Path $LocalRepository) {
		Remove-Item -Recurse -Force $LocalRepository 
	}
	
	if (Test-Path $TargetsDir) {
		Remove-Item -Recurse -Force $TargetsDir 
	}
}

Task Build -Depends Init {
   Exec { msbuild $SolutionFile /t:Build /p:Configuration=$Configuration /v:Quiet /p:OutDir=$OutDir }
}

Task Restore -Depends Init {
    Restore-Solution -SolutionFile $SolutionFile -Repositories $Repositories | Out-Null
}

Task Publish -Depends Init {
	Package-Solution -SolutionFile $SolutionFile -Version $Version -DestinationDirectory $env:TEMP | Publish-Package -Repository $LocalRepository -DeleteSource | Out-Null
}