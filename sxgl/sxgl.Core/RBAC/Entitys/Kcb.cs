using Furion.DatabaseAccessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Core.RBAC.Entitys;

public class Kcb : IEntity
{
    [Key]
    [DisplayName("主键")]
    public int Id { get; set; }

    [DisplayName("课程编号")]
    public string Kcbh { get; set; }

    [DisplayName("课程名称")]
    public string Name { get; set; }
    [DisplayName("学分")]
    public string Xf { get; set; }
    [DisplayName("开课学院")]
    public int Xyid{ get; set; }

    [DisplayName("开课专业")]
    public int Zyid { get; set; }

    [DisplayName("开课学期")]

    public int Xqid{ get; set; }

    [DisplayName("是否删除")]
    public bool IsDeleted{ get; set; } = false;
}
