# Multi-stage build for the Meridian API. Build context is the REPO ROOT
# (see docker-compose.yml) so the project references resolve.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, on the project files only, so the layer is cached until a
# package reference actually changes.
COPY src/Meridian.Domain/Meridian.Domain.csproj src/Meridian.Domain/
COPY src/Meridian.Dal/Meridian.Dal.csproj src/Meridian.Dal/
COPY src/Meridian.Bll/Meridian.Bll.csproj src/Meridian.Bll/
COPY src/Meridian.Api/Meridian.Api.csproj src/Meridian.Api/
RUN dotnet restore src/Meridian.Api/Meridian.Api.csproj

COPY src/ src/
RUN dotnet publish src/Meridian.Api/Meridian.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# The aspnet base image listens on 8080 by default (non-root).
EXPOSE 8080
ENTRYPOINT ["dotnet", "Meridian.Api.dll"]
