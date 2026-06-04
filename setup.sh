#!/bin/bash
if [ ! -d "Server/wwwroot/Content/Win-x64" ]; then
    echo "Content folder missing!"
    echo "Please run build-clients.ps1 on a Windows machine first,"
    echo "then copy the files to Server/wwwroot/Content/ and Remotely-*.zip to Server/wwwroot/Content/"
    exit 1
else
    echo "Content already exists, skipping."
fi
