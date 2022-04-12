FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.3

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 5010/tcp
ENV ASPNETCORE_URLS http://*:5010

ENTRYPOINT ["./init.sh"]
