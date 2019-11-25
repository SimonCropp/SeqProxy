using VerifyXunit;
using Xunit;

[GlobalSetUp]
public static class GlobalSetup
{
    public static void Setup()
    {
        Global.ScrubMachineName();
    }
}