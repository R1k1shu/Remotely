#!/bin/bash
if [ ! -d "Server/wwwroot/Content/Win-x64" ]; then
    echo "Content folder missing!"
    echo "1. Run build-clients.ps1 on a Windows machine"
    echo "2. Copy Remotely-*.zip to Server/wwwroot/Content/"
    echo "3. Copy Win-x64/* to Server/wwwroot/Content/Win-x64/"
    echo "4. Copy Linux-x64/* to Server/wwwroot/Content/Linux-x64/"
    exit 1
else
    echo "Content already exists, skipping."
fi
