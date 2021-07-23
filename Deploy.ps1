$myDocumentsPath = [Environment]::GetFolderPath("MyDocuments")
$secretsPath = Join-Path -Path $myDocumentsPath -ChildPath "/Secrets/Anubis.json"
$secrets = Get-Content $secretsPath | Out-String | ConvertFrom-Json

$projectPath = "./src/Anubis/Anubis.csproj"
$publishPath = "./Publish"

$deployFolder = $publishPath + "/*"

$root = "root@" + $secrets.hostName;
$deploy = $secrets.userName + "@" + $secrets.hostName + ":" + $secrets.deployPath

Write-Output "Executing as $root"
Write-Output "Deploy to $deploy"

$installPath = $publishPath + "/Install.sh"

dotnet publish $projectPath -p:PublishProfile="linux-arm" --output $publishPath -p:DebugType=None

# Swap Install.sh from dos to unix line endings to make it runnable in Linux
$installRaw = Get-Content $installPath -raw

$installRaw -replace "`r", "" | Set-Content -NoNewline $installPath

Write-Output "Stopping service..."
ssh -i $secrets.keyPath $root "systemctl stop Anubis"

Write-Output "Deploying service..."
scp -i $secrets.keyPath $deployFolder $deploy

Write-Output "Restarting service..."
ssh -i $secrets.keyPath $root "systemctl start Anubis"