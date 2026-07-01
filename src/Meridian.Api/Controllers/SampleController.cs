using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

// Thin controller: calls BLL services and returns results. No business logic,
// no validation logic, no mapping here.
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    // TODO: inject BLL service(s) and add actions per spec.
}
