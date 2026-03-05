FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RAG.API/RAG.API.csproj", "RAG.API/"]
COPY ["RAG.Application/RAG.Application.csproj", "RAG.Application/"]
COPY ["RAG.Domain/RAG.Domain.csproj", "RAG.Domain/"]
COPY ["RAG.Infrastructure/RAG.Infrastructure.csproj", "RAG.Infrastructure/"]

RUN dotnet restore "RAG.API/RAG.API.csproj"

COPY . .

RUN dotnet publish "RAG.API/RAG.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RAG.API.dll"]