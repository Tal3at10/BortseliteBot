# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY CustomsBot/CustomsBot.csproj CustomsBot/
RUN dotnet restore CustomsBot/CustomsBot.csproj

# Copy everything else and build
COPY CustomsBot/ CustomsBot/
WORKDIR /src/CustomsBot
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Railway will set PORT env variable)
ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "CustomsBot.dll"]
