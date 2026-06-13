FROM mcr.microsoft.com/dotnet/aspnet:9.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 5151

ENV ASPNETCORE_URLS=http://+:5151

FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-ltsc2022 AS build
ARG configuration=Release
WORKDIR /src
COPY ["ApiPrueba/ApiPrueba/ApiPrueba.csproj", "ApiPrueba/ApiPrueba/"]
RUN dotnet restore "ApiPrueba\ApiPrueba\ApiPrueba.csproj"
COPY . .
WORKDIR "/src/ApiPrueba/ApiPrueba"
RUN dotnet build "ApiPrueba.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ApiPrueba.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiPrueba.dll"]
