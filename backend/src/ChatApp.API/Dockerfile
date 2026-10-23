FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ChatApp.API/ChatApp.API.csproj", "ChatApp.API/"]
RUN dotnet restore "ChatApp.API/ChatApp.API.csproj"

COPY . .
WORKDIR "/src/ChatApp.API"
RUN dotnet publish "ChatApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChatApp.API.dll"]