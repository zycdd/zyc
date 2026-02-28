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

namespace sxgl.Core.RBAC.Entitys;

[Table("teab")]
public class Teab : IEntity
{
    [Key]
    [DisplayName("主键")]
    public int Id { get; set; }

    [DisplayName("工号")]
    public string Gh { get; set; }

    [DisplayName("名称")]
    public string Name { get; set; }

    [DisplayName("联系方式")]
    public string Phone { get; set; }

    [DisplayName("所属学院")]
    public int Xyid { get; set; }

    [DisplayName("所属专业")]
    public int Zyid { get; set; }

    [DisplayName("是否删除")]
    public bool IsDeleted { get; set; } = false;

}
