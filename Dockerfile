# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./ 
RUN dotnet restore

# Copy the rest of the project
COPY . ./ 

# Build and publish
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published files
COPY --from=build /app/publish .

# Expose port 5000
EXPOSE 5000

# Start the app
ENTRYPOINT ["dotnet", "ChatService.dll"]
