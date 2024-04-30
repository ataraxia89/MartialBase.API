FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["MartialBase.API/MartialBase.API.csproj", "MartialBase.API/"]
COPY ["MartialBase.API.Data/MartialBase.API.Data.csproj", "MartialBase.API.Data/"]
COPY ["MartialBase.API.Tools/MartialBase.API.Tools.csproj", "MartialBase.API.Tools/"]

RUN dotnet restore "MartialBase.API/MartialBase.API.csproj"
COPY . .
WORKDIR "/src/MartialBase.API"
RUN dotnet build "MartialBase.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MartialBase.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MartialBase.API.dll"]