# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y curl
USER app
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj", "ExpenseService/ExpenseService.Api/"]
COPY ["ExpenseTracker/ExpenseTracker.Api/ExpenseTracker.Api.csproj", "ExpenseTracker/ExpenseTracker.Api/"]
COPY ["ExpenseTracker/ExpenseTracker.Application/ExpenseTracker.Application.csproj", "ExpenseTracker/ExpenseTracker.Application/"]
COPY ["ExpenseService/ExpenseService.Application/ExpenseService.Application.csproj", "ExpenseService/ExpenseService.Application/"]
COPY ["ExpenseService/ExpenseService.Infrastructure/ExpenseService.Infrastructure.csproj", "ExpenseService/ExpenseService.Infrastructure/"]
RUN dotnet restore "./ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj"
COPY . .
WORKDIR "/src/ExpenseService/ExpenseService.Api"
RUN dotnet build "./ExpenseService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExpenseService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpenseService.Api.dll"]