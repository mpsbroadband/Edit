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
	if($env:Teamcity_nuget_feed){
		$BuildRepository = $env:Teamcity_nuget_feed
	}
	else{
		$BuildRepository = "http://build.mpsdev.com/httpAuth/app/nuget/v1/FeedService.svc/"
	}
	$BuildRepositoryDev = "http://devbuild.mpsdev.com/httpAuth/app/nuget/v1/FeedService.svc/"
	$SkipTests = $false
	$BaconDll = "$PSScriptRoot\Build\Bacon.dll"
	[string[]]$RemoteRepositories = $BuildRepository, "https://packages.nuget.org/api/v2"
	[string[]]$RemoteRepositoriesDev = $BuildRepositoryDev, "https://packages.nuget.org/api/v2"
	$MajorMinorVersion = $env:MajorMinorVersion
	if ($env:Teamcity_dotCover_home -ne $null){
		$dotCover=$env:Teamcity_dotCover_home+ "\dotCover.exe"
	}
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
 		$script:Version = "$MajorMinorVersion.$BuildCounter"
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
	
	if($env:Teamcity_dotCover_home -ne $null){
		$targetArguments=$null;
		
		Get-ChildItem -Recurse -Include "*Tests.dll" | ForEach-Object { 
			$file = $_
			
			if ($file.FullName.Contains($OutDir)) {
				if($targetArguments -eq $null) {
					$targetArguments=$file.FullName;
				}
				else {
					$targetArguments+=' ' + $file.FullName;
				}
			}
		}
		$dotCoverOutput="F:\temp\dotCover\Edit\" + $Version + ".dcvr";
		$mSpecExecutable=$mspec.FullName;
		$analyseXml=[xml]"<CoverageParams>  <TargetExecutable>$mSpecExecutable</TargetExecutable>  <TargetArguments>$targetArguments</TargetArguments>  <TargetWorkingDir>$PSScriptRoot</TargetWorkingDir>  <Output>$dotCoverOutput</Output>  <Filters>        <IncludeFilters>      <FilterEntry>        <ModuleMask>*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry>    </IncludeFilters>    <ExcludeFilters>      <FilterEntry>        <ModuleMask>*Raven*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry> <FilterEntry>        <ModuleMask>*Esent*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry><FilterEntry>        <ModuleMask>*Lucene*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry><FilterEntry>        <ModuleMask>*Spatial4n*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry><FilterEntry>        <ModuleMask>*Edit*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry>    <FilterEntry>        <ModuleMask>*Bus*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry><FilterEntry>        <ModuleMask>*Common*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry><FilterEntry>        <ModuleMask>EventStore</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry><FilterEntry>        <ModuleMask>NLog*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry><FilterEntry>        <ModuleMask>WamsApi*</ModuleMask>        <ClassMask>*</ClassMask>        <FunctionMask>*</FunctionMask>      </FilterEntry></ExcludeFilters>    </Filters>  <ReportType>xml</ReportType></CoverageParams>" ;

		$analyseXmlPath="$PSScriptRoot\Build\analyse.xml";

		$analyseXml.Save($analyseXmlPath);
		Invoke-Expression "& `"$dotCover`" cover $analyseXmlPath"
		$targetArguments=$null;

		# pass message to teamcity to process code coverage
		TeamCity-ImportDotNetCoverageResult 'dotcover' $dotCoverOutput
	}
	else{
		Get-ChildItem -Recurse -Include "*Tests.dll" | ForEach-Object { 
			$file = $_
			
			if ($file.FullName.Contains($OutDir)) {
				exec { & $mspec $file }
			}
		}
	}
}

Task Publish -Depends Build, Test {
	Package-Solution -SolutionFile $SolutionFile -Version $Version -DestinationDirectory $env:TEMP -FilesBaseDirectory $OutDir | Publish-Package -Repository $LocalRepository -DeleteSource | Out-Null
}