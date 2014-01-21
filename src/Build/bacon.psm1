function Bacon-Init([string]$packageSource, [string]$packageSourceName, [string]$baconDll) {
	$FrameworkBin = "$env:WinDir\Microsoft.NET\Framework64\v4.0.30319"
	
	if ((Get-PSSnapin -Registered -Name Bacon -ErrorAction SilentlyContinue) -eq $null) {
		Write-Host "Bacon:`tInstalling..." -ForegroundColor Gray
		Start-Process "$FrameworkBin\InstallUtil.exe" -ArgumentList "/i $baconDll" -Verb runAs -Wait
	}
	
	Import-Module $baconDll -DisableNameChecking -Scope Global
	
	if ((Get-PackageSource -Name $packageSourceName) -eq $null) {
		Write-Host "Bacon:`tConfiguring package source..." -ForegroundColor Gray
		Add-PackageSource -Name $packageSourceName -Source $packageSource | Out-Null
	}
}