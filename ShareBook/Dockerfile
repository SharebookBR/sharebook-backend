FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app


COPY . ./
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.2-aspnetcore-runtime
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT Development
WORKDIR /app
COPY --from=build-env  /app/ShareBook.Api/out/ .

ENTRYPOINT ["dotnet", "ShareBook.Api.dll"]
