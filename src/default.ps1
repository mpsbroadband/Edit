Import-Module .\Build\Bacon.dll -DisableNameChecking

Properties {
	$SolutionFile = "Edit.sln"
	$TargetsDir = "Target"
	$OutDir = "$TargetsDir\bin"
	$Configuration = "Debug"
	$Platform = "Any CPU"
	$LocalRepository = ($env:LOCALAPPDATA + "\.bacon")
	$Version = (Get-Date -Format "yyyyMMdd.HHmm.ss")
	[string[]]$Repositories = $LocalRepository, "https://packages.nuget.org/api/v2"
}

Task Default -Depends Test

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

Task Restore -Depends Init {
    Restore-Solution -SolutionFile $SolutionFile -Repositories $Repositories | Out-Null
}

Task Build -Depends Restore {
   Exec { msbuild $SolutionFile /p:Configuration=$Configuration /p:Platform=$Platform /p:OutDir=$OutDir /p:OutputPath=$OutDir }
}

Task Test -Depends Build {
	Get-ChildItem -Recurse -Include "*Tests.dll" | ForEach-Object { 
		$file = $_
		
		if ($file.FullName.Contains($OutDir)) {
			exec { packages\Machine.Specifications\tools\mspec-clr4.exe $file.FullName }
		}
	}
}

Task Publish -Depends Test {
	Package-Solution -SolutionFile $SolutionFile -Version $Version -DestinationDirectory $env:TEMP -FilesBaseDirectory $OutDir | Publish-Package -Repository $LocalRepository -DeleteSource | Out-Null
}