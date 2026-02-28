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

public class AuthSeed : IEntityTypeBuilder<Auth>, IEntitySeedData<Auth>
{
    public void Configure(EntityTypeBuilder<Auth> entityBuilder, DbContext dbContext, Type dbContextLocator)
    {
        entityBuilder.HasKey(u => u.Id);
        entityBuilder.HasIndex(u => u.Auth1);
        entityBuilder.HasIndex(u => u.Role).IsUnique();
        entityBuilder.Property(u => u.Role).IsRequired().HasMaxLength(100);
    }

    public IEnumerable<Auth> HasData(DbContext dbContext, Type dbContextLocator)
    {
        return new List<Auth>
        {
            new Auth { Id = 1,Role = "Admin",Auth1 = "system,user,xy,tea,stu,zy,xq,kc" },
            new Auth { Id = 2,Role = "Jwc",Auth1 = "" },
             new Auth { Id = 3,Role = "Xy",Auth1 = "" },
            new Auth{ Id = 4 ,Role = "Teacher",Auth1 =""},
            new Auth{Id = 5 ,Role = "Student",Auth1 = ""}
        };
    }
}
