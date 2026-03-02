FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY AnarchyChess.Api/AnarchyChess.Api.csproj AnarchyChess.Api/AnarchyChess.Api.csproj
COPY AnarchyChess.Ai.Service/AnarchyChess.Ai.Service.csproj AnarchyChess.Ai.Service/AnarchyChess.Ai.Service.csproj
COPY AnarchyChess.Ai/AnarchyChess.Ai.csproj AnarchyChess.Ai/AnarchyChess.Ai.csproj
COPY AnarchyChess.EngineShared/AnarchyChess.EngineShared.csproj AnarchyChess.EngineShared/AnarchyChess.EngineShared.csproj

RUN dotnet restore "./AnarchyChess.Api/AnarchyChess.Api.csproj"

COPY AnarchyChess.Api/ AnarchyChess.Api/
COPY AnarchyChess.Ai.Service/ AnarchyChess.Ai.Service/
COPY AnarchyChess.Ai/ AnarchyChess.Ai/
COPY AnarchyChess.EngineShared/ AnarchyChess.EngineShared/

WORKDIR /src/AnarchyChess.Api
RUN dotnet publish "./AnarchyChess.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AnarchyChess.Api.dll"]
