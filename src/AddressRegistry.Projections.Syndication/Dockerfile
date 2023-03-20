FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.4

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="address-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

ENTRYPOINT ["./init.sh"]
