FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Acme.SaaS.API/Acme.SaaS.API.csproj", "src/Acme.SaaS.API/"]
COPY ["src/Acme.SaaS.Application/Acme.SaaS.Application.csproj", "src/Acme.SaaS.Application/"]
COPY ["src/Acme.SaaS.Domain/Acme.SaaS.Domain.csproj", "src/Acme.SaaS.Domain/"]
COPY ["src/Acme.SaaS.Infrastructure/Acme.SaaS.Infrastructure.csproj", "src/Acme.SaaS.Infrastructure/"]
RUN dotnet restore "src/Acme.SaaS.API/Acme.SaaS.API.csproj"
COPY . .
WORKDIR "/src/src/Acme.SaaS.API"
RUN dotnet build "Acme.SaaS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Acme.SaaS.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Acme.SaaS.API.dll"]
