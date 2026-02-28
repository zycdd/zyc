using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sxgl.Core.RBAC.Entitys;
using sxgl.Core.RBAC.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Core.RBAC.SeedData;

public class UserSeed : IEntityTypeBuilder<User>,IEntitySeedData<User>
{
    public void Configure(EntityTypeBuilder<User> entityBuilder,DbContext dbContext,Type dbContextLocator)
    {
        entityBuilder.HasKey(u  => u.Id);
        entityBuilder.HasIndex(u => u.UserName);
        entityBuilder.HasIndex(u => u.Role);
    }

    public IEnumerable<User> HasData(DbContext dbContext, Type dbContextLocator)
    {
        return new List<User>
        {
            new User { Id = 1, UserName = "admin", Password = DataEncryption.Sha1Encrypt("123456"),Role = "Admin" }
        };
    }
}
