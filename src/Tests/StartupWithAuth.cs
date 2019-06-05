using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class StartupWithAuth
{
    #region StartupWithAuth

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseSeq(useAuthorizationService: true);
        #endregion
    }
}