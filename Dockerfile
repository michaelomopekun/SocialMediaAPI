FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["SocialMediaAPI.csproj", "./"]
RUN dotnet restore

# Copy the rest of the code
COPY . .
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /src/out .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:$PORT

# Start the app
ENTRYPOINT ["dotnet", "SocialMediaAPI.dll"]