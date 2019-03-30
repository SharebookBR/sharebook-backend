FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app


COPY *.csproj ./
RUN dotnet restore ShareBook.Api/*.csproj
RUN dotnet publish ShareBook.Api/*.csproj -c Release -o out


COPY . ./
RUN dotnet publish -c Release -o out


FROM microsoft/dotnet:2.2-aspnetcore-runtime
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "ShareBook.Api.dll"]