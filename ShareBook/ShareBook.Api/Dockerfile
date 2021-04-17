#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ShareBook.Api/ShareBook.Api.csproj", "ShareBook.Api/"]
COPY ["ShareBook.Service/ShareBook.Service.csproj", "ShareBook.Service/"]
COPY ["ShareBook.Repository/ShareBook.Infra.Data.csproj", "ShareBook.Repository/"]
COPY ["ShareBook.Domain/ShareBook.Domain.csproj", "ShareBook.Domain/"]
COPY ["ShareBook.Helper/ShareBook.Helper.csproj", "ShareBook.Helper/"]
COPY ["Sharebook.Jobs/Sharebook.Jobs.csproj", "Sharebook.Jobs/"]
COPY ["ShareBook.Infra.CrossCutting.Identity/ShareBook.Infra.CrossCutting.Identity.csproj", "ShareBook.Infra.CrossCutting.Identity/"]
RUN dotnet restore "ShareBook.Api/ShareBook.Api.csproj"
COPY . .
WORKDIR "/src/ShareBook.Api"
RUN dotnet build "ShareBook.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShareBook.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShareBook.Api.dll"]