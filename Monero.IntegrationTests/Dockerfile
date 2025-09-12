FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS builder

WORKDIR /source

COPY . .

ARG CONFIGURATION_NAME=Release
RUN cd Monero.IntegrationTests && dotnet build --configuration ${CONFIGURATION_NAME} /p:CI_TESTS=true /p:RazorCompileOnBuild=true
RUN dotnet tool install --global JetBrains.DotCover.CommandLineTools --version 2025.1.6
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /source/Monero.IntegrationTests
ENV CONFIGURATION_NAME=${CONFIGURATION_NAME}
ENTRYPOINT ["./docker-entrypoint.sh"]