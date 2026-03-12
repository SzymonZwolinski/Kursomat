FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ArchitectureTester.csproj", "./"]
RUN dotnet restore "ArchitectureTester.csproj"
COPY . .
RUN dotnet build "ArchitectureTester.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArchitectureTester.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
