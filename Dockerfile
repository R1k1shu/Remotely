FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Устанавливаем git и Node.js (нужен для libman)
RUN apt-get update && apt-get install -y git nodejs npm && \
    dotnet tool install -g Microsoft.Web.LibraryManager.Cli && \
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /root/.bashrc

ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .

# Восстанавливаем JS-библиотеки через libman
RUN cd Server && libman restore

# Публикуем
RUN dotnet publish Server/Server.csproj \
    -c Release \
    -o /app/publish \
    --no-self-contained \
    -p:RestoreUseStaticGraphEvaluation=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Remotely.Server.dll"]
