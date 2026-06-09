FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Configurar que escuche en el puerto 7012
ENV ASPNETCORE_URLS=http://+:7012
EXPOSE 7012

ENTRYPOINT ["dotnet", "LogiSlot.dll"]
