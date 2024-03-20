FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.2-bookworm-slim-amd64

# create work dir and set permissions as WORKDIR sets permissions as root
RUN mkdir /app && chown -R app:app /app
WORKDIR /app

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="address-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 5014/tcp
ENV ASPNETCORE_URLS http://*:5014

# switch to created user
USER app

ENTRYPOINT ["./init.sh"]
