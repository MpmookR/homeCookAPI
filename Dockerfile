# Use the official .NET SDK as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["homeCookAPI.csproj", "./"]
RUN dotnet restore "./homeCookAPI.csproj"

COPY . .
WORKDIR "/src/"
RUN dotnet publish "homeCookAPI.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "homeCookAPI.dll"]
