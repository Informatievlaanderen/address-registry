FROM mcr.microsoft.com/dotnet/core/runtime-deps:2.2.4

LABEL maintainer "Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

ENTRYPOINT ["./AddressRegistry.Api.Legacy"]
