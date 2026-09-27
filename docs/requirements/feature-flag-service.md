# Developer Exercise: Feature Flag Service

## Overview

Implement a simple Feature Flag Service using C# and .NET.

The service should allow consumers to determine whether a feature is enabled for a specific user.

The exercise is intentionally limited in scope and should take approximately 1-2 hours to complete.

## Functional Requirements

### Feature Definition

```csharp
public class FeatureFlag
{
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public int RolloutPercentage { get; set; }
}
```

Example:

```json
{
  "name": "NewDashboard",
  "enabled": true,
  "rolloutPercentage": 20
}
```

### Feature Evaluation

Implement an API endpoint:

```http
GET /api/features/{featureName}/enabled?userId=123
```

The endpoint should return:

```json
{
  "enabled": true
}
```

or

```json
{
  "enabled": false
}
```

### Evaluation Rules

1. If the feature does not exist, return 404 Not Found.
2. If Enabled = false, the feature is disabled for all users.
3. If Enabled = true and RolloutPercentage = 100, the feature is enabled for all users.
4. If Enabled = true and RolloutPercentage < 100, the feature is only enabled for a subset of users.
5. The evaluation must be deterministic.

## Technical Requirements

- .NET 10 (or .NET 11 RC)
- ASP.NET Core Web API
- Dependency Injection
- Proper separation of concerns
- Unit tests
- In-memory repository

## Deliverables

- Source code
- Build/run instructions
- Automated tests
