# Build and runtime dependencies
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
RUN apt-get update && apt-get install -y libgdiplus libfontconfig1 libc6-dev tzdata

# Environment
ENV TZ=Europe/Prague
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Build
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
COPY . .
RUN dotnet nuget add source -n "MyGet_DiscordNET_Prerelease" https://www.myget.org/F/discord-net/api/v3/index.json

FROM build AS publish
RUN dotnet publish "/src/Grillbot/Grillbot.csproj" -c Release -o /app/publish

# Final image build
FROM base AS final
EXPOSE 80
WORKDIR /app
COPY --from=publish /app/publish .
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["dotnet", "Grillbot.dll"]
