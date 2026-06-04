#!/bin/bash
# Скачиваем Content из оригинального образа если его нет
if [ ! -d "Server/wwwroot/Content/Win-x64" ]; then
    echo "Downloading Content from original image..."
    docker run --rm -d --name remotely-orig immybot/remotely:latest sleep 30
    docker cp remotely-orig:/app/wwwroot/Content/. Server/wwwroot/Content/
    docker stop remotely-orig
    echo "Done."
else
    echo "Content already exists, skipping."
fi
