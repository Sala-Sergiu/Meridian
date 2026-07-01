using Meridian.Api.Authorization;
using Meridian.Bll.Dtos;
using Meridian.Bll.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

// Thin read controller for HR-owned onboarding templates. Any authenticated
// user can read; HR-only writes arrive in a later slice behind Policies.HrWrite.
[ApiController]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    private readonly IOnboardingTemplateService _templates;

    public TemplatesController(IOnboardingTemplateService templates)
    {
        _templates = templates;
    }

    [HttpGet]
    [Authorize(Policy = Policies.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<OnboardingTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OnboardingTemplateDto>>> GetTemplates(CancellationToken cancellationToken)
    {
        return Ok(await _templates.GetTemplatesAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.CanRead)]
    [ProducesResponseType(typeof(OnboardingTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OnboardingTemplateDto>> GetTemplate(int id, CancellationToken cancellationToken)
    {
        var template = await _templates.GetTemplateAsync(id, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }
}
