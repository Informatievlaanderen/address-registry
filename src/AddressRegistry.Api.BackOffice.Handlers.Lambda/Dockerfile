FROM public.ecr.aws/lambda/dotnet:9.2025.04.21.12 AS base

# Stage to download and extract the Datadog .NET tracer
# https://docs.datadoghq.com/serverless/aws_lambda/installation/dotnet/?tab=containerimage
FROM alpine:3.21 AS dd-tracer
ARG TRACER_VERSION=3.15.0

RUN apk add --no-cache wget tar \
 && wget https://github.com/DataDog/dd-trace-dotnet/releases/download/v${TRACER_VERSION}/datadog-dotnet-apm-${TRACER_VERSION}.tar.gz \
 && mkdir /opt/datadog \
 && tar -C /opt/datadog -xzf datadog-dotnet-apm-${TRACER_VERSION}.tar.gz

FROM base

WORKDIR /var/task

# This COPY command copies the .NET Lambda project's build artifacts from the host machine into the image.
# The source of the COPY should match where the .NET Lambda project publishes its build artifacts. If the Lambda function is being built
# with the AWS .NET Lambda Tooling, the `--docker-host-build-output-dir` switch controls where the .NET Lambda project
# will be built. The .NET Lambda project templates default to having `--docker-host-build-output-dir`
# set in the aws-lambda-tools-defaults.json file to "bin/Release/lambda-publish".
#
# Alternatively Docker multi-stage build could be used to build the .NET Lambda project inside the image.
# For more information on this approach checkout the project's README.md file.
COPY "bin/Release/lambda-publish"  .

COPY --from=public.ecr.aws/datadog/lambda-extension:latest /opt/. /opt/
COPY --from=dd-tracer /opt/datadog /opt/datadog

ENV DD_TRACE_OTEL_ENABLED=false
ENV DD_PROFILING_ENABLED=false
ENV DD_SERVERLESS_APPSEC_ENABLED=false

ENV AWS_LAMBDA_EXEC_WRAPPER=/opt/datadog_wrapper

CMD ["AddressRegistry.Api.BackOffice.Handlers.Lambda::AddressRegistry.Api.BackOffice.Handlers.Lambda.Function::Handler"]
