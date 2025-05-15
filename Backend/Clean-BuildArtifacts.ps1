cd $PSScriptRoot;
[Environment]::CurrentDirectory = $PSScriptRoot;

$projectFolders = Get-Childitem -Filter "*.csproj" -Recurse | Select-Object -ExpandProperty Directory

$removeFolders = $projectFolders | Get-ChildItem -Directory | Where-Object {$_.Name -ceq 'bin' -or $_.Name -ceq 'obj'}
foreach($folder in $removeFolders){
    $folder | Remove-Item -Verbose -Recurse -ErrorAction SilentlyContinue
}