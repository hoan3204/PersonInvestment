# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PersonalInvestmentSystem.Web/PersonalInvestmentSystem.Web.csproj", "PersonalInvestmentSystem.Web/"]
RUN dotnet restore "PersonalInvestmentSystem.Web/PersonalInvestmentSystem.Web.csproj"

COPY . .
WORKDIR "/src/PersonalInvestmentSystem.Web"
RUN dotnet publish "PersonalInvestmentSystem.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PersonalInvestmentSystem.Web.dll"]