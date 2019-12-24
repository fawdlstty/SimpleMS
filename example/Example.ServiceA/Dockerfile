#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["example/Example.ServiceA/Example.ServiceA.csproj", "example/Example.ServiceA/"]
RUN dotnet restore "example/Example.ServiceA/Example.ServiceA.csproj"
COPY . .
WORKDIR "/src/example/Example.ServiceA"
RUN dotnet build "Example.ServiceA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Example.ServiceA.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Example.ServiceA.dll"]