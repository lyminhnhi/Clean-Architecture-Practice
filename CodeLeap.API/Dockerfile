# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CodeLeap.API/CodeLeap.API.csproj", "CodeLeap.API/"]
COPY ["CodeLeap.Application/CodeLeap.Application.csproj", "CodeLeap.Application/"]
COPY ["CodeLeap.Domain/CodeLeap.Domain.csproj", "CodeLeap.Domain/"]
COPY ["CodeLeap.Infrastructure/CodeLeap.Infrastructure.csproj", "CodeLeap.Infrastructure/"]

RUN dotnet restore "CodeLeap.API/CodeLeap.API.csproj"

COPY . .
WORKDIR "/src/CodeLeap.API"
RUN dotnet publish -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "CodeLeap.API.dll"]
