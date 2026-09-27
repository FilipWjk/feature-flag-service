# Feature Flag Service Implementation

This document describes the current implementation, the main technical
decisions, and the steps used to build the service.

## Current State

The repository contains a .NET 10 ASP.NET Core Web API with a small, explicit
architecture:

- `FeatureFlags.Core` contains the domain model and evaluation rules.
- `FeatureFlags.Api` contains HTTP, configuration, dependency injection, and
  the OpenAPI endpoint.
- `FeatureFlags.Core.Tests` covers domain behavior.
- `FeatureFlags.Api.Tests` covers HTTP behavior with an in-memory test server.

The service evaluates a feature for one user. It does not create users, persist
rollout history, or provide user-specific overrides. The caller supplies the
user ID.

## Implementation Approach

The implementation was developed in small layers so that each responsibility
could be tested independently:

1. The required feature-flag behavior and HTTP responses were defined first.
2. The domain model was separated from ASP.NET Core and configuration concerns.
3. Configuration is validated while the application starts, before requests are
   accepted.
4. Deterministic rollout bucketing was implemented independently of storage and
   HTTP.
5. The API controller was added as a thin adapter around the domain service.
6. The OpenAPI document was made the contract source, with response models
   generated from it during the build.
7. Unit tests and API integration tests were added for the business and HTTP
   boundaries respectively.

This order keeps the evaluation rules testable without a web server and makes
the API layer responsible only for transport concerns.

## Project Structure

```text
src/FeatureFlags.Core/                Domain model, bucketing, and service
src/FeatureFlags.Api/                 HTTP endpoint and configuration
src/FeatureFlags.Api/Infrastructure/  Configuration-backed repository
tests/FeatureFlags.Core.Tests/        Domain unit tests
tests/FeatureFlags.Api.Tests/         HTTP integration tests
```

The layers have distinct responsibilities:

1. `FeatureFlagsController` validates HTTP input and translates service results
   into HTTP responses.
2. `FeatureFlagService` applies the feature evaluation rules without an HTTP
   dependency.
3. `IFeatureFlagRepository` defines the data access contract.
4. `InMemoryFeatureFlagRepository` provides immutable, case-insensitive reads.

## Configuration and Startup

Flags are defined in [appsettings.json](../src/FeatureFlags.Api/appsettings.json):

```json
"FeatureFlags": [
  { "Name": "NewDashboard", "Enabled": true, "RolloutPercentage": 20 },
  { "Name": "DarkMode", "Enabled": true, "RolloutPercentage": 100 },
  { "Name": "LegacyExport", "Enabled": false, "RolloutPercentage": 100 },
  { "Name": "BetaSearch", "Enabled": true, "RolloutPercentage": 0 }
]
```

`FeatureFlagDefinition` keeps `Name` and `RolloutPercentage` nullable while
binding configuration so that missing values can be distinguished from valid
values. `ToFeatureFlag()` then converts the definition into the validated core
model.

Startup fails with a descriptive error when:

- the name is empty;
- the rollout percentage is missing or outside `0..100`; or
- two names differ only by case.

The repository is registered as a singleton. It loads the flags once, rejects
duplicates with `StringComparer.OrdinalIgnoreCase`, and stores the result in a
`FrozenDictionary`. The dictionary cannot change after startup and is optimized
for concurrent read-only lookups.

The application resolves the repository immediately after building the host.
That deliberate resolution makes invalid configuration fail during startup,
not on the first request.

## Request Flow

For a request such as:

```http
GET /api/features/NewDashboard/enabled?userId=102
```

the following happens:

1. The controller rejects a missing or blank `userId` with `400 Bad Request`
   and `ValidationProblemDetails`.
2. The service retrieves the feature through `IFeatureFlagRepository`.
3. An unknown feature becomes `404 Not Found` with `ProblemDetails`.
4. `Enabled = false` disables the feature for every user.
5. `Enabled = true` with `RolloutPercentage = 100` enables it for every user.
6. All other enabled flags use `RolloutBucketer` to calculate a stable bucket.
7. The controller returns `{ "enabled": true }` or `{ "enabled": false }`.

## Deterministic Partial Rollout

The service uses SHA-256 in [RolloutBucketer.cs](../src/FeatureFlags.Core/RolloutBucketer.cs).
The input combines the lowercase feature name with the user ID:

```text
newdashboard:102
```

The first four hash bytes are read as a big-endian unsigned integer and reduced
with modulo 100:

```text
bucket = first four hash bytes as UInt32 % 100
```

For a 20% rollout, buckets `0..19` are enabled. For example:

```text
normalized input: newdashboard:102
SHA-256:          e5b103c4007d00aee3b82f21363762e79602e54fe04913069091c11858e4bbd7
first 4 bytes:    e5b103c4 = 3,853,583,300
bucket:           3,853,583,300 % 100 = 0
```

The same feature, rollout, and user always produce the same result across
requests, restarts, and servers. Lowercasing makes `NewDashboard` and
`newdashboard` use the same bucket. Including the feature name keeps bucketing
independent between features. `string.GetHashCode()` is deliberately not used
because its value can differ between .NET processes.

Increasing a rollout from 20% to 50% keeps users in buckets `0..19` enabled and
adds buckets `20..49`; it does not reshuffle the existing assignments.

## OpenAPI and Generated Models

[openapi.yaml](../src/FeatureFlags.Api/openapi.yaml) is the API contract and the
source of truth for the response shape. It is also served at
`/openapi/v1.yaml` in Development and displayed by Swagger UI at `/swagger`.

During `dotnet build`, `NSwag.MSBuild` generates
`Contracts/Generated/ApiModels.g.cs` from `openapi.yaml`. The generated file is
compiled before the API build but is ignored by Git because it can be recreated
on every platform from the versioned contract. A clean checkout therefore needs
only `dotnet restore` followed by `dotnet build`.

Swagger UI and the OpenAPI endpoint are enabled only in the Development
environment, keeping the production HTTP surface smaller.

## Testing

The tests cover the two main boundaries separately:

- [FeatureFlags.Core.Tests](../tests/FeatureFlags.Core.Tests) verifies unknown
  features, disabled and full rollouts, deterministic case-insensitive
  bucketing, and invalid rollout values.
- [FeatureFlags.Api.Tests](../tests/FeatureFlags.Api.Tests) verifies successful
  responses, validation and not-found errors, and Development documentation
  endpoints.

Run the complete test suite with:

```bash
dotnet test FeatureFlagService.sln
```

## Scope Boundaries

The current implementation intentionally does not include persistent storage,
audit logging, per-user exceptions, authentication, or an administration API.
