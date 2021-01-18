using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

#pragma warning disable CS8644 // Type does not implement interface member.
public class MockCookieCollection :
    Dictionary<string, string>,
    IRequestCookieCollection
{
    public new ICollection<string> Keys => base.Keys;
}