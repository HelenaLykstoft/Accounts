# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file
COPY *.sln ./

# Copy all project files
COPY Accounts.API/Accounts.API.csproj Accounts.API/
COPY Accounts.Tests/Accounts.Tests.csproj Accounts.Tests/
COPY Accounts.Core/Accounts.Core.csproj Accounts.Core/
COPY Accounts.Infrastructure/Accounts.Infrastructure.csproj Accounts.Infrastructure/

# Restore dependencies for all projects
RUN dotnet restore Accounts.sln

# Copy the remaining files and build the application
COPY . ./
RUN dotnet publish Accounts.API/Accounts.API.csproj -c Release -o /app/out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "Accounts.API.dll"]