# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files for restore
COPY ["PLC.Api/PLC.Api.csproj", "PLC.Api/"]
COPY ["PLC.Application/PLC.Application.csproj", "PLC.Application/"]
COPY ["PLC.Infrastructure/PLC.Infrastructure.csproj", "PLC.Infrastructure/"]
COPY ["PLC.Domain/PLC.Domain.csproj", "PLC.Domain/"]

# Restore dependencies
WORKDIR /src/PLC.Api
RUN dotnet restore "PLC.Api.csproj"

# Copy all source files
WORKDIR /src
COPY ["PLC.Api/", "PLC.Api/"]
COPY ["PLC.Application/", "PLC.Application/"]
COPY ["PLC.Infrastructure/", "PLC.Infrastructure/"]
COPY ["PLC.Domain/", "PLC.Domain/"]

# Build
WORKDIR /src/PLC.Api
RUN dotnet build "PLC.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
WORKDIR /src/PLC.Api
RUN dotnet publish "PLC.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cloud Run uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PLC.Api.dll"]
