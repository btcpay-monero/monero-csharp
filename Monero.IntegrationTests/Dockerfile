FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS builder

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet build --no-restore

CMD ["dotnet", "test", "--filter", "TestMoneroRpcConnection|TestMoneroDaemonRpc", "--no-build", "--logger:trx"]
