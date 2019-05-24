using Xunit;
using Xunit.Abstractions;

public class Samples :
    XunitLoggingBase
{
    [Fact]
    public void Foo()
    {
    }

    public Samples(ITestOutputHelper output) :
        base(output)
    {
    }
}