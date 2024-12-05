FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all files for the build context
COPY . .

# Debug: List all files to verify the structure
RUN echo "Listing files in /src:" && ls -R /src

# Restore dependencies
RUN dotnet restore "BlackjackWebsocket/BlackjackWebsocket/BlackjackWebsocket.csproj"

# Build the project
RUN dotnet build "BlackjackWebsocket/BlackjackWebsocket/BlackjackWebsocket.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "BlackjackWebsocket/BlackjackWebsocket/BlackjackWebsocket.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlackjackWebsocket.dll"]
