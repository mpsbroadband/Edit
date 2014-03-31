Import-Module .\Build\bacon.psm1 -DisableNameChecking
Import-Module .\Build\teamcity.psm1 -DisableNameChecking

Properties {
	$SolutionFile = "Edit.sln"
	$TargetsDir = "Target"
	$PackagesDir = "packages"
	$OutDir = "$TargetsDir\bin\"
	$Configuration = "Debug"
	$Platform = "Any CPU"
	$TargetProfile = "Cloud"
	$BuildCounter = $null
	$LocalRepository = ($env:LOCALAPPDATA + "\.bacon")
	$BuildRepository = "http://build.mpsdev.com/httpAuth/app/nuget/v1/FeedService.svc/"
	$SkipTests = $false
	$BaconDll = "$PSScriptRoot\Build\Bacon.dll"
	[string[]]$RemoteRepositories = $BuildRepository, "https://packages.nuget.org/api/v2"
}

Task Default -Depends Test

if ($env:TEAMCITY_VERSION) {
	TaskSetup {
		TeamCity-ReportBuildProgress "Running task $($psake.context.Peek().currentTaskName)"
	}
}

Task Init {
	if ($BuildCounter -eq $null) {
		$script:Version = (Get-Date -Format "yyyyMMdd.HHmm.ss")
	} else {
		$script:Version = "1.0.$BuildCounter"
	}
	
	Write-Host "Solution:`t$SolutionFile" -ForegroundColor Gray
	Write-Host "Configuration:`t$Configuration" -ForegroundColor Gray
	Write-Host "Version:`t$Version" -ForegroundColor Gray

	if ($env:TEAMCITY_VERSION) {
		TeamCity-SetBuildNumber $Version
	}
	
	Bacon-Init $BuildRepository "MPS" $BaconDll
	Import-Module $baconDll -DisableNameChecking -Scope Global
}

Task Clean -Depends Init {
	if (Test-Path $LocalRepository) {
		Remove-Item -Recurse -Force $LocalRepository 
	}
	
	if (Test-Path $TargetsDir) {
		Remove-Item -Recurse -Force $TargetsDir 
	}
	
	if (Test-Path $PackagesDir) {
		Remove-Item -Recurse -Force $PackagesDir 
	}
}

Task Restore -Depends Init {
    Restore-Solution -SolutionFile $SolutionFile -LocalRepository $LocalRepository -RemoteRepositories $RemoteRepositories | Out-Null
}

Task Build -Depends Restore {
   Exec { msbuild $SolutionFile /t:Build /p:Configuration=$Configuration /p:TargetProfile=$TargetProfile /p:Platform=$Platform /p:OutDir=$OutDir /p:OutputPath=$OutDir }
   Exec { msbuild $SolutionFile /t:Publish /p:Configuration=$Configuration /p:TargetProfile=$TargetProfile /p:Platform=$Platform /p:OutDir=$OutDir /p:OutputPath=$OutDir }
}

Task Test -Depends Build -Precondition { return $SkipTests -ne $true } {
	$mspec = (Get-ChildItem -Recurse -Include "mspec-clr4.exe")[0]
	
	if ($mspec -eq $null) {
		throw "Could not locate mspec."
	}
	
	Get-ChildItem -Recurse -Include "*Tests.dll" | ForEach-Object { 
		$file = $_
		
		if ($file.FullName.Contains($OutDir)) {
			exec { & $mspec $file }
		}
	}
}

Task Publish -Depends Build, Test {
	Package-Solution -SolutionFile $SolutionFile -Version $Version -DestinationDirectory $env:TEMP -FilesBaseDirectory $OutDir | Publish-Package -Repository $LocalRepository -DeleteSource | Out-Null
}