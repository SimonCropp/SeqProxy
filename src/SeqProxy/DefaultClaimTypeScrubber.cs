namespace SeqProxy
{
    public static class DefaultClaimTypeScrubber
    {
        public static string Scrub(string claimType)
        {
            Guard.AgainstNullOrEmpty(claimType, nameof(claimType));
            var lastIndexOf = claimType.LastIndexOf("/");
            if (lastIndexOf == -1)
            {
                return claimType;
            }

            return claimType.Substring(lastIndexOf + 1);
        }
    }
}