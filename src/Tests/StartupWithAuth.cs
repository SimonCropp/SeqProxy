#pragma warning disable IDE0022
#pragma warning disable CA1822
public class StartupWithAuth
{
    #region StartupWithAuth

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseSeq(useAuthorizationService: true);
        #endregion
    }
}