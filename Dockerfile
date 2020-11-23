#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
RUN apt-get update && apt-get install -y libgdiplus libfontconfig1 libc6-dev tzdata
ENV TZ=Europe/Prague
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
COPY . .
RUN dotnet nuget add source -n "MyGet_DiscordNET_Prerelease" https://www.myget.org/F/discord-net/api/v3/index.json
RUN dotnet restore "/src/Grillbot/Grillbot.csproj"
RUN dotnet build "/src/Grillbot/Grillbot.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "/src/Grillbot/Grillbot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
ENTRYPOINT ["dotnet", "Grillbot.dll"]
