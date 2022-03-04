FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Forge.DeploymentAgent/Forge.DeploymentAgent.fsproj", "Forge.DeploymentAgent/"]
RUN dotnet restore "Forge.DeploymentAgent/Forge.DeploymentAgent.fsproj"
COPY . .
WORKDIR "/src/Forge.DeploymentAgent"
RUN dotnet build "Forge.DeploymentAgent.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forge.DeploymentAgent.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Forge.DeploymentAgent.dll"]
