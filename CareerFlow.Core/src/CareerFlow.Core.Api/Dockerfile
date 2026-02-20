FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG PACKAGE_TOKEN
ENV PACKAGE_TOKEN=$PACKAGE_TOKEN
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/CareerFlow.Core.Api/CareerFlow.Core.Api.csproj", "src/CareerFlow.Core.Api/"]
COPY ["src/CareerFlow.Core.Domain/CareerFlow.Core.Domain.csproj", "src/CareerFlow.Core.Domain/"]
COPY ["src/CareerFlow.Core.Application/CareerFlow.Core.Application.csproj", "src/CareerFlow.Core.Application/"]
COPY ["src/CareerFlow.Core.Rabbit.Events/CareerFlow.Core.Rabbit.Events.csproj", "src/CareerFlow.Core.Rabbit.Events/"]
COPY ["src/CareerFlow.Core.Infrastructure/CareerFlow.Core.Infrastructure.csproj", "src/CareerFlow.Core.Infrastructure/"]
RUN dotnet restore "src/CareerFlow.Core.Api/CareerFlow.Core.Api.csproj"
ENV PACKAGE_TOKEN=
COPY . .
WORKDIR "/src/src/CareerFlow.Core.Api"
RUN dotnet build "./CareerFlow.Core.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CareerFlow.Core.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CareerFlow.Core.Api.dll"]