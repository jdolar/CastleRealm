# Use the official .NET 9 SDK image for build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Api.csproj", "./"]
RUN dotnet restore "./Api.csproj"

# Copy all source files and build the project
COPY . .
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

# Use the official .NET 9 ASP.NET runtime image for runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 80
EXPOSE 80

# Entry point to run the API
ENTRYPOINT ["dotnet", "Api.dll"]
