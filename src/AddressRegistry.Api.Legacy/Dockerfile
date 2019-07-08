FROM mcr.microsoft.com/dotnet/core/runtime-deps:2.2.4

LABEL maintainer "agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 5002/tcp
ENV ASPNETCORE_URLS http://*:5002

ENTRYPOINT ["./init.sh"]
