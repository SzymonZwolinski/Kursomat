FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Kopiujemy całą solucję (ze wszystkimi modułami) do Dockera
COPY . .

# 2. Przechodzimy do folderu Hosta
# Dzięki temu .csproj poprawnie znajdzie ścieżki do modułów (np. "..\Modular.Modules.Courses")
WORKDIR /src/Modular.Host

# 3. Budujemy projekt Hosta
RUN dotnet restore "Modular.Host.csproj"
RUN dotnet build "Modular.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Modular.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Modular.Host.dll"]