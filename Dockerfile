FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

COPY ["src/RaythaZero.Domain/RaythaZero.Domain.csproj", "src/RaythaZero.Domain/"]
COPY ["src/RaythaZero.Application/RaythaZero.Application.csproj", "src/RaythaZero.Application/"]
COPY ["src/RaythaZero.Infrastructure/RaythaZero.Infrastructure.csproj", "src/RaythaZero.Infrastructure/"]
COPY ["src/RaythaZero.Web/RaythaZero.Web.csproj", "src/RaythaZero.Web/"]
COPY ["RaythaZero.sln", ""]

ARG DOTNET_RESTORE_CLI_ARGS=
RUN dotnet restore "RaythaZero.sln" $DOTNET_RESTORE_CLI_ARGS

COPY . .
RUN dotnet build "RaythaZero.sln" -c Release --no-restore

RUN dotnet publish -c Release --no-build -o /app "src/RaythaZero.Web/RaythaZero.Web.csproj"

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ARG BUILD_NUMBER=
ENV BUILD_NUMBER=$BUILD_NUMBER

ENTRYPOINT ["dotnet", "RaythaZero.Web.dll"]