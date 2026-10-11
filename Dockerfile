FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CampusEats.Api/CampusEats.Api.csproj CampusEats.Api/
RUN dotnet restore CampusEats.Api/CampusEats.Api.csproj

COPY CampusEats.Api/ CampusEats.Api/
RUN dotnet publish CampusEats.Api/CampusEats.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet CampusEats.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
