FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SchoolManagement.API/SchoolManagement.API.csproj SchoolManagement.API/
COPY SchoolManagement.BLL/SchoolManagement.BLL.csproj SchoolManagement.BLL/
COPY SchoolManagement.DAL/SchoolManagement.DAL.csproj SchoolManagement.DAL/
COPY SchoolManagement.Common/SchoolManagement.Common.csproj SchoolManagement.Common/

RUN dotnet restore SchoolManagement.API/SchoolManagement.API.csproj

COPY . .
RUN dotnet publish SchoolManagement.API/SchoolManagement.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SchoolManagement.API.dll"]
