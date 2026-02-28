using Furion.DatabaseAccessor;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Core.RBAC.Entitys;

[Table("stub")]
public class Stub : IEntity
{
    [Key]
    [DisplayName("主键")]
    public int Id { get; set; }

    [DisplayName("学号")]
    public string Xh { get; set; }

    [DisplayName("姓名")]
    public string Name { get; set; }

    [DisplayName("所属学院")]
    public int Xyid { get; set; }

    [DisplayName("所属专业")]
    public int  Zyid { get; set; }
    [DisplayName("联系方式")]
    public string Phone {  get; set; }

    [DisplayName("师范类型")]
    public string Sflx { get; set; }

    [DisplayName("是否删除")]
    public bool IsDeleted { get; set; } = false;
}
