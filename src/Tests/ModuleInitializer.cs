﻿public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.ScrubMachineName();
        VerifyDiffPlex.Initialize();
    }
}