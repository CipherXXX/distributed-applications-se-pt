FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SkillForge.API/SkillForge.API.csproj", "SkillForge.API/"]
COPY ["SkillForge.Application/SkillForge.Application.csproj", "SkillForge.Application/"]
COPY ["SkillForge.Domain/SkillForge.Domain.csproj", "SkillForge.Domain/"]
COPY ["SkillForge.Infrastructure/SkillForge.Infrastructure.csproj", "SkillForge.Infrastructure/"]
RUN dotnet restore "SkillForge.API/SkillForge.API.csproj"
COPY . .
RUN dotnet build "SkillForge.API/SkillForge.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SkillForge.API/SkillForge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkillForge.API.dll"]
