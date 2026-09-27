# Feature Flag Service

A small ASP.NET Core Web API (.NET 10) that evaluates whether a feature is enabled
for a specific user, with deterministic percentage-based rollout.

## Project layout

```
src/FeatureFlags.Core/       Validated domain model and evaluation rules
src/FeatureFlags.Api/        HTTP endpoint, configuration and OpenAPI contract
tests/FeatureFlags.Core.Tests/  Domain unit tests
tests/FeatureFlags.Api.Tests/   HTTP integration tests
```

## Documentation

- [Requirements](docs/requirements/feature-flag-service.md)
- [Implementation](docs/Implementation.md)
- [Architecture diagram](docs/architecture/README.md)

## Requirements

- .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
- Git

### Operating systems

The project supports all three major desktop operating systems:

| OS      | Terminal                       | Notes                                                  |
| ------- | ------------------------------ | ------------------------------------------------------ |
| macOS   | Terminal with Zsh or Bash      | Install the .NET 10 SDK and Git.                       |
| Linux   | Bash or another POSIX shell    | Install the .NET 10 SDK and Git for your distribution. |
| Windows | PowerShell or Windows Terminal | Install the .NET 10 SDK and Git for Windows.           |

The `dotnet`, `git`, `restore`, `build`, `test`, and `run` commands below are the
same on all three operating systems.

## Getting started

Clone the public repository and enter its directory:

```bash
git clone https://github.com/FilipWjk/feature-flag-service.git
cd feature-flag-service
```

Check the installed SDK:

```bash
dotnet --version
```

The version must be `10.x`.

Restore dependencies and build the solution:

```bash
dotnet restore
dotnet build
```

Run the automated tests:

```bash
dotnet test
```

Expected result: all tests pass.

## Run the API

```bash
dotnet run --project src/FeatureFlags.Api
```

With the default Development launch profile, the Swagger UI opens automatically
in your browser at startup. The API continues running in the terminal in the
background, so Swagger can send requests to the running service. Keep the
terminal open while testing; press `Ctrl+C` to stop the API.

If the browser does not open automatically, navigate to:

- Swagger UI: http://localhost:5095/swagger
- OpenAPI contract: http://localhost:5095/openapi/v1.yaml
- Contract source: [src/FeatureFlags.Api/openapi.yaml](src/FeatureFlags.Api/openapi.yaml)

Swagger UI and the contract endpoint are available only in Development.

You can test the API manually either through Swagger UI or by opening
[FeatureFlags.Api.http](src/FeatureFlags.Api/FeatureFlags.Api.http) in VS Code
with a REST Client extension. The `.http` file contains requests for full and
partial rollouts, case-insensitive lookup, validation errors, and unknown
features.

## Test

```bash
dotnet test
```

## API

```
GET /api/features/{featureName}/enabled?userId=123
```

Response:

```json
{ "enabled": true }
```

### Evaluation rules

1. Unknown feature -> `404 Not Found`.
2. Missing `userId` -> `400 Bad Request`.
3. `Enabled = false` -> disabled for everyone.
4. `Enabled = true` and `RolloutPercentage = 100` -> enabled for everyone.
5. `Enabled = true` and `RolloutPercentage < 100` -> enabled for a deterministic subset.

### Determinism

Partial rollouts are deterministic: the same feature, rollout, and user always
produce the same result across requests and restarts. Feature-name lookup is
case-insensitive, and each feature has its own rollout assignment. The technical
hashing algorithm and a worked example are described in
[Implementation](docs/Implementation.md).

### Seeded flags

| Name         | Enabled | Rollout | Behaviour                |
| ------------ | ------- | ------- | ------------------------ |
| DarkMode     | true    | 100     | Enabled for everyone     |
| LegacyExport | false   | 100     | Disabled for everyone    |
| NewDashboard | true    | 20      | Deterministic 20% subset |
| BetaSearch   | true    | 0       | Disabled for everyone    |

Flags are loaded from `appsettings.json` at startup. Empty names, duplicate names
(case-insensitive), missing rollout percentages, and rollout values outside `0..100`
stop startup with a descriptive error.

## Manual testing

Open [src/FeatureFlags.Api/FeatureFlags.Api.http](src/FeatureFlags.Api/FeatureFlags.Api.http)
in VS Code (REST Client extension) and run each request. It covers full rollout,
disabled flag, in/out of a partial rollout, determinism, case-insensitive lookup,
`404` and `400` cases.
