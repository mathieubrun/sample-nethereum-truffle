FROM microsoft/dotnet:2.1-sdk-alpine3.7 AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./NethClient/*.csproj ./NethClient/
RUN dotnet restore NethClient

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/out NethClient

# build runtime image
FROM microsoft/dotnet:2.1-runtime-alpine3.7
WORKDIR /app

COPY --from=build-env /app/out ./

ENTRYPOINT ["dotnet", "NethClient.dll"]