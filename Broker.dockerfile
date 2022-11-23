FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 6000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Broker/Broker.csproj", "Broker/"]
RUN dotnet restore "Broker/Broker.csproj"
COPY . .
WORKDIR "/src/Broker"
RUN dotnet build "Broker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Broker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Broker.dll"]
