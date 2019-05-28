using System;

static class Scrubber
{
    public static string Scrub(string s)
    {
        return s.Replace(Environment.MachineName, "TheMachineName");
    }
}