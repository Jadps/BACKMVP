FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MVP.WebAPI/MVP.WebAPI.csproj", "MVP.WebAPI/"]
COPY ["MVP.Application/MVP.Application.csproj", "MVP.Application/"]
COPY ["MVP.Infrastructure/MVP.Infrastructure.csproj", "MVP.Infrastructure/"]
COPY ["MVP.Domain/MVP.Domain.csproj", "MVP.Domain/"]

RUN dotnet restore "MVP.WebAPI/MVP.WebAPI.csproj"

COPY . .
WORKDIR "/src/MVP.WebAPI"
RUN dotnet publish "MVP.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

USER $APP_UID

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MVP.WebAPI.dll"]