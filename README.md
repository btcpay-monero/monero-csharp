<div align="center">

  [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/btcpay-monero/monero-csharp/dotnet.yml?branch=master)](https://github.com/btcpay-monero/monero-csharp/actions)
  [![Codacy Badge](https://app.codacy.com/project/badge/Grade/ec5951b790c647db8961ec228c48b739)](https://app.codacy.com/gh/btcpay-monero/monero-csharp/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
  [![Codacy Badge](https://app.codacy.com/project/badge/Coverage/ec5951b790c647db8961ec228c48b739)](https://app.codacy.com/gh/btcpay-monero/monero-csharp/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_coverage)
  [![Matrix rooms](https://img.shields.io/badge/%F0%9F%92%AC%20Matrix-%23btcpay--monero-blue)](https://matrix.to/#/#btcpay-monero:matrix.org)
</div>

# Monero C# Bindings

Library for interacting with Monero daemon and wallet RPCs.

## Code formatting

We use the **unmodified** standardized `.editorconfig` from .NET SDK. Run `dotnet new editorconfig --force` to apply the latest version.

To enforce formatting for the whole project, run `dotnet format monero-csharp.sln --verbosity diagnostic`

To enforce custom analyzer configuration options, we do use global _AnalyzerConfig_ `.globalconfig` file.

## Documentation

We use [DocFX](https://github.com/dotnet/docfx) to generate documentation. 

To build and serve the docs locally:
````
docfx metadata docs/docfx.json      # Generate metadata
docfx build docs/docfx.json --serve # Build and serve documentation
````

# License

[MIT](LICENSE.md)