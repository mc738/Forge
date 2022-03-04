FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Forge.WebApp/Forge.WebApp.csproj", "Forge.WebApp/"]
RUN dotnet restore "Forge.WebApp/Forge.WebApp.csproj"
COPY . .
WORKDIR "/src/Forge.WebApp"
RUN dotnet build "Forge.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forge.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Forge.WebApp.dll"]
