FROM microsoft/dotnet:2.2.0-runtime-deps

LABEL maintainer "Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

ENTRYPOINT ["./AddressRegistry.Projections.Legacy"]
