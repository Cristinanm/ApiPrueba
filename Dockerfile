FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ApiPrueba/ApiPrueba/ApiPrueba.csproj", "ApiPrueba/ApiPrueba/"]
RUN dotnet restore "ApiPrueba/ApiPrueba/ApiPrueba.csproj"

COPY . .
WORKDIR "/src/ApiPrueba/ApiPrueba"
RUN dotnet publish "ApiPrueba.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "ApiPrueba.dll"]