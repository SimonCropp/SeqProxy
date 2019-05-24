using System.IO;
using Microsoft.AspNetCore.Mvc;

public class TestController :
    ControllerBase
{
    [HttpGet]
    [Route("test")]
    public IActionResult Test()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "test.html");
        return PhysicalFile(file, "text/html");
    }
}