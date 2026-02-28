using Furion.DatabaseAccessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Core.RBAC.Entitysl;

[Table("zyb")]
public class Zyb : IEntity
{
    [DisplayName("主键")]
    [Key]
    public int Id { get; set; }

    [DisplayName("专业代码")]
    public string Dm { get; set; }

    [DisplayName("专业名称")]
    public string Name { get; set; }

    [DisplayName("所属学院")]
    public int Xyid { get; set; }

    [DisplayName("是否删除")]
    public bool IsDeleted { get; set; } = false;
}
