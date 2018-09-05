[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$tag = (Invoke-WebRequest -Uri https://api.github.com/repos/fsprojects/Paket/releases | ConvertFrom-Json)[0].tag_name
$uri = " https://github.com/fsprojects/Paket/releases/download/" + $tag + "/paket.bootstrapper.exe"
Invoke-WebRequest $uri  -OutFile .paket/paket.exe