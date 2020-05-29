param(
	[Parameter(mandatory=$true)]
	[string] $newVersion="",
	[string] $baseDirectory=".."
)

$pattern = '<PackageReference Include="Optano.Algorithm.Tuner" Version="(.*)" />'
$csprojFiles = Get-ChildItem $baseDirectory *.csproj -rec

#iterate all project files
foreach ($file in $csprojFiles)
{
	(Get-Content $file.PSPath) | ForEach-Object{
		# iterate all lines
		if($_ -match $pattern){
			# Match - replace
			'    <PackageReference Include="Optano.Algorithm.Tuner" Version="'+ $newVersion + '" />' 
		} 
		else 
		{
			# no match
			$_
		}
	} | Set-Content $file.PSPath
}