FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Debug with ls command to check contents
RUN echo "Checking contents of /src before copying project file:" && ls -R /src

COPY ["BlackjackAPI/BlackjackAPI.csproj", "BlackjackAPI/"]  
RUN dotnet restore "BlackjackAPI/BlackjackAPI.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "BlackjackAPI/BlackjackAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "BlackjackAPI/BlackjackAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlackjackAPI.dll"]
