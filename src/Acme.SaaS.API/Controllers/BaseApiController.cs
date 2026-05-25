using Acme.SaaS.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ToActionResult<T>(ApiResponse<T> response)
    {
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
