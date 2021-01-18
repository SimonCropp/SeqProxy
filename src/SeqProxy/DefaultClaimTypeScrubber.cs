namespace SeqProxy
{
    /// <summary>
    /// Used for scrubbing claims when no other scrubber is defined.
    /// </summary>
    public static class DefaultClaimTypeScrubber
    {
        /// <summary>
        /// Get the string after the last /.
        /// </summary>
        public static string Scrub(string claimType)
        {
            Guard.AgainstNullOrEmpty(claimType, nameof(claimType));
            var lastIndexOf = claimType.LastIndexOf('/');
            if (lastIndexOf == -1)
            {
                return claimType;
            }

            return claimType.Substring(lastIndexOf + 1);
        }
    }
}