FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.5

LABEL maintainer "agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 5004/tcp
ENV ASPNETCORE_URLS http://*:5004

ENTRYPOINT ["./init.sh"]
