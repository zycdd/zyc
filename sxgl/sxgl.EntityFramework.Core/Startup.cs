using Furion;
using Microsoft.Extensions.DependencyInjection;

namespace sxgl.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "sxgl.Database.Migrations");
    }
}
