FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AnarchyChess.Api/AnarchyChess.Api.csproj", "./"]
COPY ["AnarchyChess.EngineShared/AnarchyChess.EngineShared.csproj", "./"]
RUN dotnet restore "./AnarchyChess.Api.csproj"
COPY AnarchyChess.Api/ AnarchyCHess.Api/
COPY AnarchyChess.EngineShared/ AnarchyCHess.EngineShared/
RUN dotnet publish "./AnarchyChess.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AnarchyChess.Api.dll"]
