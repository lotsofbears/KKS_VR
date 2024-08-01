if ($PSScriptRoot -match '.+?\\bin\\?') {
    $dir = $PSScriptRoot + "\"
}
else {
    $dir = $PSScriptRoot + "\bin\"
}

$solutionDir = $dir + "..\"

$copy = $dir + "\copy\BepInEx\plugins\KKS_VR"

$ver = "v" + (Get-ChildItem -Path ($dir) -Filter ("KKS_MainGameVR.dll") -Recurse -Force)[0].VersionInfo.FileVersion.ToString() -replace "([\d+\.]+?\d+)[\.0]*$", '${1}'

Remove-Item -Force -Path ($dir + "\copy") -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $copy

Copy-Item -Path ($dir + "\KKS_*.dll") -Destination $copy -Recurse -Force

Copy-Item -Path ($solutionDir + "\VRGIN_OpenXR\bin\*.dll") -Destination $copy -Force

Copy-Item -Path ($solutionDir + "\README.md") -Destination $copy -Recurse -Force
Copy-Item -Path ($solutionDir + "\LICENSE") -Destination $copy -Recurse -Force

New-Item -ItemType Directory -Force -Path ($copy + "\Images")
Copy-Item -Path ($solutionDir + "\Images\*") -Destination ($copy + "\Images") -Recurse -Force


Copy-Item -Path ($solutionDir + "\VRGIN_OpenXR\Libs\*.dll") -Destination $copy -Force

New-Item -ItemType Directory -Force -Path ($dir + "\copy\CharaStudio_Data")
Copy-Item -Path ($solutionDir + "\VRGIN_OpenXR\Libs\_Data\*") -Destination ($dir + "\copy\CharaStudio_Data") -Recurse -Force

New-Item -ItemType Directory -Force -Path ($dir + "\copy\KoikatsuSunshine_Data")
Copy-Item -Path ($solutionDir + "\VRGIN_OpenXR\Libs\_Data\*") -Destination ($dir + "\copy\KoikatsuSunshine_Data") -Recurse -Force


Compress-Archive -Path ($dir + "\copy\*") -Force -CompressionLevel "Optimal" -DestinationPath ($dir +"KKS_VR_" + $ver + ".zip")


Remove-Item -Force -Path ($dir + "\copy") -Recurse -ErrorAction SilentlyContinue