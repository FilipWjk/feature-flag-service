using FeatureFlags.Api.Contracts;
using FeatureFlags.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/features")]
public sealed class FeatureFlagsController(FeatureFlagService featureFlagService) : ControllerBase
{
    [HttpGet("{featureName}/enabled")]
    [ProducesResponseType<FeatureEnabledResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<FeatureEnabledResponse> IsEnabled(string featureName, [FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["userId"] = ["The userId query parameter is required and must not be empty."],
            }));
        }

        var evaluation = featureFlagService.Evaluate(featureName, userId);
        if (!evaluation.FeatureFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Feature '{featureName}' does not exist.");
        }

        return Ok(new FeatureEnabledResponse(evaluation.IsEnabled));
    }
}