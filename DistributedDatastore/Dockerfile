FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DistributedDatastore/DistributedDatastore.csproj", "DistributedDatastore/"]
RUN dotnet restore "DistributedDatastore/DistributedDatastore.csproj"
COPY . .
WORKDIR "/src/DistributedDatastore"
RUN dotnet build "DistributedDatastore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DistributedDatastore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DistributedDatastore.dll"]
