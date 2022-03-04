FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Forge.App/Forge.App.fsproj", "Forge.App/"]
RUN dotnet restore "Forge.App/Forge.App.fsproj"
COPY . .
WORKDIR "/src/Forge.App"
RUN dotnet build "Forge.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forge.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Forge.App.dll"]
