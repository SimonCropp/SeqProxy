public class SampleController :
    ControllerBase
{
    [HttpGet]
    [Route("sample")]
    public IActionResult Sample()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "sample.html");
        return PhysicalFile(file, "text/html");
    }

    [HttpGet]
    [Route("sample.js")]
    public IActionResult SampleJS()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "sample.js");
        return PhysicalFile(file, "text/html");
    }
}