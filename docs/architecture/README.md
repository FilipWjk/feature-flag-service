# Architecture Diagram

`feature-flag-service.architecture.html` is a standalone, interactive architecture diagram for this project.

## Source and generation

- Source specification: [`feature-flag-service.architecture.json`](feature-flag-service.architecture.json)
- Generated with **Archify 2.17**
- Diagram type: `architecture`
- Schema version: `1`
- Output: self-contained HTML with inline SVG, styles, scripts, and fonts

The JSON specification is the editable source. The HTML is the generated artifact and can be opened directly in a browser without a server or build step.

## What it shows

The diagram follows the system from an HTTP request to the feature decision:

```text
Calling system -> Controller -> FeatureFlagService -> Repository
                                      |
                               RolloutBucketer
```

It also shows the startup path from `appsettings.json` into the read-only in-memory repository.

The HTML contains two focused views:

- **Evaluate request**: request validation, lookup, rollout bucketing, and response
- **Load configuration**: startup validation and immutable repository initialization

The components and relationships are based on the implementation in `src/FeatureFlags.Api` and `src/FeatureFlags.Core`.
The diagram is explanatory documentation; it is not part of the API runtime.
