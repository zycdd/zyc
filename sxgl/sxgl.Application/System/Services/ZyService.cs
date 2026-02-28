using sxgl.Application.System.Dtos;
using sxgl.Core.RBAC.Entitys;
using sxgl.Core.RBAC.Entitysl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Application.System.Services;

public class ZyService : IDynamicApiController, ITransient
{
    private readonly IRepository<Zyb> _ZyRep;
    private readonly IRepository<Xyb> _XyRep;
    public ZyService(IRepository<Zyb> zyRep, IRepository<Xyb> xyRep)
    {
        _ZyRep = zyRep;
        _XyRep = xyRep;
    }

    //查看全部专业信息
    [HttpGet("GetAllZy")]
    public async Task<dynamic> GetAllZy()
    {
        var zy = await _ZyRep.Where(z => z.IsDeleted == false).ToArrayAsync();
        return zy;
    }
    //添加一个专业信息
    [HttpPost("AddZy")]
    public async Task<dynamic> AddZy(ZyDTO input)
    {
        var zy1 = await _ZyRep.Where(z => z.Dm == input.Dm && z.IsDeleted == false).FirstOrDefaultAsync();
        if (zy1 != null) {
            return new { code = 400, message = "该专业代码已经存在" };
        }
        var zy = new Zyb
        {
            Dm = input.Dm,
            Name = input.Name,
            Xyid = input.Xyid
        };
        var result = await _ZyRep.InsertAsync(zy);
        return new {code = 200 , message = "添加成功" ,result.Entity};
    }

    //删除专业信息
    [HttpPost("DeleteZy")]
    public async Task<dynamic> DeleteZy(ZyDTO input)
    {
        var zy = await _ZyRep.Where(z => z.Id == input.Id && z.IsDeleted == false).FirstOrDefaultAsync();
        var result = await _ZyRep.DeleteAsync(zy);
        return new {code = 200 ,message = "删除成功",result.Entity};
    }

    //修改一条专业信息
    [HttpPost("UpdateZy")]
    public async Task<dynamic> UpdateZy (ZyDTO input)
    {
        var zy = await _ZyRep.Where(z => z.Id == input.Id && z.IsDeleted == false).FirstOrDefaultAsync();
        var zy1 = await _ZyRep.Where(z => z.Dm == input.Dm && z.IsDeleted == false).FirstOrDefaultAsync();
        if (zy1 != null && zy1.Dm != zy.Dm)
        {
            return new { code = 400, message = "该专业代码已经存在" };
        }
        zy.Dm = input.Dm;
        zy.Name  = input.Name;
        zy.Xyid = input.Xyid;
        var result = await _ZyRep.UpdateAsync(zy);
        return new {code = 200 ,message = "更新成功",result.Entity} ;
    }
    //根据学院id获取学院名称
    [HttpPost("GetXyByXyid")]
    public async Task<dynamic> GetXyByXyid(ZyDTO input)
    {
        var xy = await _XyRep.Where(x => x.Id == input.Xyid && x.IsDeleted == false).FirstOrDefaultAsync();
        return xy.Name;
    }
    //获取全部学院信息
    [HttpGet("GetAllXy")]
    public async Task<dynamic> GetAllXy()
    {
        var xy = await _XyRep.Where(x => x.IsDeleted == false).ToListAsync();
        if (xy == null)
        {
            return new { Code = 404, Message = "找不到学院信息" };
        }
        else
        {
            return xy;
        }
    }
}
