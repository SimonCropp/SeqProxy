using VerifyXunit;

public static class ModuleInitializer
{
    public static void Initialize()
    {
        Global.ScrubMachineName();
    }
}