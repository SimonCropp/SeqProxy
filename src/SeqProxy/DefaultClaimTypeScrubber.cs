namespace SeqProxy;

/// <summary>
/// Used for scrubbing claims when no other scrubber is defined.
/// </summary>
public static class DefaultClaimTypeScrubber
{
    /// <summary>
    /// Get the string after the last /.
    /// </summary>
    public static CharSpan Scrub(CharSpan claimType)
    {
        Guard.AgainstEmpty(claimType, nameof(claimType));
        var lastIndexOf = claimType.LastIndexOf('/');
        if (lastIndexOf == -1)
        {
            return claimType;
        }

        return claimType[(lastIndexOf + 1)..];
    }
}