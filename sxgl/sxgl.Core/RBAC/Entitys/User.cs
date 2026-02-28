using Furion.DatabaseAccessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Core.RBAC.Entitys;

[Table("user")]
public class User : IEntity
{
    [DisplayName("主键")]
    [Key]
    public int Id { get; set; }

    [DisplayName("用户名")]
    public string UserName { get; set; }

    [DisplayName("密码")]
    public string Password { get; set; }

    [DisplayName("角色")]
    public string Role { get; set; }

    [DisplayName("是否删除")]
    public bool IsDeleted { get; set; } = false;
}
