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
WORKDIR /src
RUN dotnet publish "homeCookAPI.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app

# Copy the built files to the runtime image
COPY --from=build /app/publish .

# Ensure ASP.NET Core listens on all interfaces inside Docker
ENV ASPNETCORE_URLS="http://+:8080"

# Load environment variables from .env file
ARG DATABASE_CONNECTION
ARG JWT_KEY
ARG JWT_ISSUER
ARG SMTP_SERVER
ARG SMTP_USERNAME
ARG SMTP_PASSWORD

ENV DATABASE_CONNECTION=$DATABASE_CONNECTION
ENV JWT_KEY=$JWT_KEY
ENV JWT_ISSUER=$JWT_ISSUER
ENV SMTP_SERVER=$SMTP_SERVER
ENV SMTP_USERNAME=$SMTP_USERNAME
ENV SMTP_PASSWORD=$SMTP_PASSWORD

# Start the application
ENTRYPOINT ["dotnet", "homeCookAPI.dll"]