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

[Table("auth")]
public class Auth : IEntity
{
    [DisplayName("主键")]
    [Key]
    public int Id { get; set; }

    [DisplayName("角色")]
    public string Role { get; set; }

    [DisplayName("权限")]
    public string Auth1 { get; set; }
}
