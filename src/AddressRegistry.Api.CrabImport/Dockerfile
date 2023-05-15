FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.5

# create group & user
RUN addgroup --gid 1000 --system app && adduser --uid 1000 -system app --gid 1000

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

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

# switch to created user
USER app

ENTRYPOINT ["./init.sh"]
