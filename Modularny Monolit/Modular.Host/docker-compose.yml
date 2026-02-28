version: '3.8'

services:
  modular-api:
    image: modular-api:latest
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8081:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sql-server-modular;Database=kursomat_modular;User Id=sa;Password=TwojeSilneHaslo123!;TrustServerCertificate=True;
    depends_on:
      - sql-server-modular

  sql-server-modular:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1434:1433"
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=TwojeSilneHaslo123!