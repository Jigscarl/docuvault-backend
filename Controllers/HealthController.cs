using Microsoft.AspNetCore.Mvc;
namespace Docuvault.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return  Ok( new
    { 
        Application = "DocuVault API",
        Status = "Healthy",
        Message = "Backend is running successfully."
        
    });}
}