# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PLC.Api/PLC.Api.csproj", "PLC.Api/"]
RUN dotnet restore "PLC.Api/PLC.Api.csproj"

COPY . .
WORKDIR "/src/PLC.Api"
RUN dotnet build "PLC.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PLC.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PLC.Api.dll"]
