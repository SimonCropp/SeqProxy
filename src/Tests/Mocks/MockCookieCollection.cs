using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

public class MockCookieCollection : 
    Dictionary<string, string>,
    IRequestCookieCollection
{
    public new ICollection<string> Keys => base.Keys;
}