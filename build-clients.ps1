# Собираем десктоп-клиенты
dotnet publish Desktop.Win/Desktop.Win.csproj -c Release -r win-x64 --self-contained true -o publish/Win-x64
dotnet publish Desktop.Win/Desktop.Win.csproj -c Release -r win-x86 --self-contained true -o publish/Win-x86
dotnet publish Desktop.Linux/Desktop.Linux.csproj -c Release -r linux-x64 --self-contained true -o publish/Linux-x64

# Собираем агентов
dotnet publish Agent/Agent.csproj -c Release -r win-x64 --self-contained true -o publish/Agent-Win-x64
dotnet publish Agent/Agent.csproj -c Release -r win-x86 --self-contained true -o publish/Agent-Win-x86
dotnet publish Agent/Agent.csproj -c Release -r linux-x64 --self-contained true -o publish/Agent-Linux-x64

# Пакуем агентов в zip
Compress-Archive -Path publish/Agent-Win-x64/* -DestinationPath publish/Remotely-Win-x64.zip -Force
Compress-Archive -Path publish/Agent-Win-x86/* -DestinationPath publish/Remotely-Win-x86.zip -Force
Compress-Archive -Path publish/Agent-Linux-x64/* -DestinationPath publish/Remotely-Linux.zip -Force

Write-Host "Done! Now copy publish/ folders to server."
