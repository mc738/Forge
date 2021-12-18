FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Forge.WebApi/Forge.WebApi.fsproj", "Forge.WebApi/"]
RUN dotnet restore "Forge.WebApi/Forge.WebApi.fsproj"
COPY . .
WORKDIR "/src/Forge.WebApi"
RUN dotnet build "Forge.WebApi.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forge.WebApi.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Forge.WebApi.dll"]
