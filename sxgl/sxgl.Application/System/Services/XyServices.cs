using sxgl.Application.System.Dtos;
using sxgl.Core.RBAC.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Application.System.Services;

public class XyServices : IDynamicApiController, ITransient
{
    private readonly IRepository<Xyb> _xyRep;
    public XyServices(IRepository<Xyb> xyRep)
    {
        _xyRep = xyRep;
    }

    //查看全部学院信息
    [HttpGet("GetAllXy")]
    public async Task<dynamic> GetAllXy()
    {
        var xy = await _xyRep.Where(x => x.IsDeleted == false).ToListAsync();
        if (xy == null) {
            return new { Code = 404, Message = "找不到学院信息" };
        }
        else
        {
            return xy;
        }
    }
    //增加一条学院信息
    [HttpPost("AddXy")]
    public async Task<dynamic> AddXy(XyDTO input)
    {
        var xy1 = await _xyRep.Where(x => x.Dm == input.Dm && x.IsDeleted == false).FirstOrDefaultAsync();
        if (xy1 != null)
        {
            return new { Code = 400, Message = "学院代码已经存在" };
        }
        var xy = new Xyb
        {
            Dm = input.Dm,
            Name = input.Name
        };
        var result = await _xyRep.InsertAsync(xy);
        return new { Code = 200, Message = "添加成功", result.Entity };
    }
    //删除一条学院信息
    [HttpPost("DeleteXy")]
    public async Task<dynamic> DeleteXy(XyDTO input)
    {
        var xy = await _xyRep.Where(x => x.Id == input.Id && x.IsDeleted == false).FirstOrDefaultAsync();
        var result = await _xyRep.DeleteAsync(xy);
        return new { Code = 200, Message = "删除成功", result.Entity };
    }

    //修改一条学院信息
    [HttpPost("UpdateXy")]
    public async Task<dynamic> UpdateXy(XyDTO input)
    {
        var xy = await _xyRep.Where(x => x.Id == input.Id && x.IsDeleted == false).FirstAsync();
        var xy1 = await _xyRep.Where(x => x.Dm == input.Dm && x.IsDeleted == false).FirstOrDefaultAsync();
        if (xy1 != null && xy1.Dm != xy.Dm)
        {
            return new { Code = 400, Message = "学院代码已经存在" };
        }
       
        if (xy == null)
        {
            return new { Code = 404, Message = "找不到该学院信息" };
        }
        else
        {
            xy.Dm = input.Dm;
            xy.Name = input.Name;
            var result = await _xyRep.UpdateAsync(xy);
            return new { Code = 200, Message = "修改成功", result.Entity };
        }
    }
    //根据学院代码查找学院信息
    [HttpPost("GetXyByDm")]
    public async Task<dynamic> GetXyByDm( XyDTO input)
    {
        var xy = await _xyRep.Where(x => x.Dm.Contains(input.Dm) && x.IsDeleted == false).ToListAsync();
        if (xy == null)
        {
            return new { Code = 404, Message = "找不到对应学院信息" };
        }
        else
        {
            return xy;
        }
    }
    //根据学院名称查找学院信息
    [HttpPost("GetXyByName")]
    public async Task<dynamic> GetXyByName( XyDTO input)
    {
        var xy = await _xyRep.Where(x => x.Name.Contains(input.Name) && x.IsDeleted == false).ToListAsync();
        if (xy == null)
        {
            return new { Code = 404, Message = "找不到对应学院信息" };
        }
        else
        {
            return xy;
        }
    }
}
