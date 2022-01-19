#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["ObjectServer.API/ObjectServer.API.csproj", "ObjectServer.API/"]
RUN dotnet restore "ObjectServer.API/ObjectServer.API.csproj"
COPY . .
WORKDIR "/src/ObjectServer.API"
RUN dotnet build "ObjectServer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ObjectServer.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ObjectServer.API.dll"]