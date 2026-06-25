static class ReservedKeyValidator
{
    // Properties that SeqProxy stamps itself and that a client must never supply in a log event,
    // otherwise the client could forge them via the request body (which is appended after the
    // server-stamped prefix). Limited to keys clients never legitimately use: generic names like
    // Server/User/Application are intentionally NOT reserved, since clients send those as normal
    // log properties (e.g. 'User':'John' to render "Hello, {User}").
    public static void ThrowIfReservedKey(string line)
    {
        ThrowIfContainsKey(line, "SeqProxyId");
        ThrowIfContainsKey(line, "Claims");
    }

    static void ThrowIfContainsKey(string line, string key)
    {
        if (line.Contains($"'{key}':") ||
            line.Contains($"\"{key}\":"))
        {
            throw new($"'{key}' is reserved by SeqProxy and cannot be included in a log event.");
        }
    }
}
