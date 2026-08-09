FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/AssignmentSystem.Core/*.csproj AssignmentSystem.Core/
COPY src/AssignmentSystem.Infrastructure/*.csproj AssignmentSystem.Infrastructure/
COPY src/AssignmentSystem.Api/*.csproj AssignmentSystem.Api/
RUN dotnet restore AssignmentSystem.Api/AssignmentSystem.Api.csproj

COPY src/ .
RUN dotnet publish AssignmentSystem.Api/AssignmentSystem.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "AssignmentSystem.Api.dll"]
