# Собираем десктоп-клиенты
dotnet publish Desktop.Win/Desktop.Win.csproj -c Release -r win-x64 --self-contained true -o publish/Win-x64
dotnet publish Desktop.Win/Desktop.Win.csproj -c Release -r win-x86 --self-contained true -o publish/Win-x86
dotnet publish Desktop.Linux/Desktop.Linux.csproj -c Release -r linux-x64 --self-contained true -o publish/Linux-x64

# Собираем агентов
dotnet publish Agent/Agent.csproj -c Release -r win-x64 --self-contained true -o publish/Agent-Win-x64
dotnet publish Agent/Agent.csproj -c Release -r win-x86 --self-contained true -o publish/Agent-Win-x86
dotnet publish Agent/Agent.csproj -c Release -r linux-x64 --self-contained true -o publish/Agent-Linux-x64

# Создаём структуру с Desktop подпапкой
New-Item -ItemType Directory -Force -Path publish/full-Win-x64/Desktop
New-Item -ItemType Directory -Force -Path publish/full-Win-x86/Desktop
New-Item -ItemType Directory -Force -Path publish/full-Linux-x64/Desktop

# Агент в корень
Copy-Item publish/Agent-Win-x64/* publish/full-Win-x64/ -Recurse -Force
Copy-Item publish/Agent-Win-x86/* publish/full-Win-x86/ -Recurse -Force
Copy-Item publish/Agent-Linux-x64/* publish/full-Linux-x64/ -Recurse -Force

# Десктоп в подпапку
Copy-Item publish/Win-x64/* publish/full-Win-x64/Desktop/ -Recurse -Force
Copy-Item publish/Win-x86/* publish/full-Win-x86/Desktop/ -Recurse -Force
Copy-Item publish/Linux-x64/* publish/full-Linux-x64/Desktop/ -Recurse -Force

# Пакуем
Compress-Archive -Path publish/full-Win-x64/* -DestinationPath publish/Remotely-Win-x64.zip -Force
Compress-Archive -Path publish/full-Win-x86/* -DestinationPath publish/Remotely-Win-x86.zip -Force
Compress-Archive -Path publish/full-Linux-x64/* -DestinationPath publish/Remotely-Linux.zip -Force

Write-Host "Done! Copy publish/Remotely-*.zip to server: Server/wwwroot/Content/"
Write-Host "Copy publish/Win-x64/* to server: Server/wwwroot/Content/Win-x64/"
Write-Host "Copy publish/Linux-x64/* to server: Server/wwwroot/Content/Linux-x64/"
