using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace sxgl.EntityFramework.Core;

[AppDbContext("MySqlConnectionString", DbProvider.MySqlOfficial)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
