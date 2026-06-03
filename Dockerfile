FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y git

COPY . .

RUN dotnet publish Server/Server.csproj \
    -c Release \
    -o /app/publish \
    --no-self-contained \
    /p:EnableLibraryManager=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Remotely.Server.dll"]
