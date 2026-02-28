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

[Table("xqb")]
public class Xqb : IEntity
{
    [DisplayName("主键")]
    [Key]
    public int Id { get; set; }

    [DisplayName("学期名称")]
    public string Name { get; set; }

    [DisplayName("开始时间")]
    public DateTime DateTime { get; set; }
    [DisplayName("是否删除")]
    public bool  IsDeleted { get; set; } = false;
}
