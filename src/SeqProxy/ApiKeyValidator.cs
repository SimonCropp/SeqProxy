﻿static class ApiKeyValidator
{
    public static void ThrowIfApiKeySpecified(HttpRequest request)
    {
        if (request.Query.ContainsKey("apiKey"))
        {
            throw new("apiKey is not allowed.");
        }

        if (request.Headers.ContainsKey("X-Seq-ApiKey"))
        {
            throw new("X-Seq-ApiKey is not allowed.");
        }
    }
}