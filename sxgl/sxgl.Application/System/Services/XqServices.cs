using sxgl.Application.System.Dtos;
using sxgl.Core.RBAC.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Application.System.Services;

public class XqServices : IDynamicApiController, ITransient
{
    private readonly IRepository<Xqb> _XqRep;
    public XqServices(IRepository<Xqb> xqRep)
    {
        _XqRep = xqRep;
    }

    //查看所有学期
    [HttpGet("GetAllXq")]
    public async Task<dynamic> GetAllXq()
    {
        var xq  = await _XqRep.Where(x => true).ToArrayAsync();
        return xq;
    }

    //添加新学期
    [HttpPost("AddXq")]
    public async Task<dynamic> AddXq (XqDTO input)
    {
        var xq = new Xqb
        {
            Name = input.Name,
            DateTime = input.DateTime,
        };
        var result = await _XqRep.InsertAsync(xq);
        return new { code = 200, message = "添加成功", result.Entity };
    }

    //删除学期
    [HttpPost("DeleteXq")]
    public async Task<dynamic> DeleteXq(XqDTO input)
    {
        var xq = await _XqRep.Where(x => x.Id == input.Id).FirstOrDefaultAsync();
        var resulet = await _XqRep.DeleteAsync(xq);
        return new { code = 200, message = "删除成功", resulet.Entity };
    }

    //编辑学期
    [HttpPost("UpdateXq")]
    public async Task<dynamic> UpdateXq(XqDTO input)
    {
        var xq = await _XqRep.Where(x => x.Id == input.Id).FirstOrDefaultAsync();
        xq.Name = input.Name;
        xq.DateTime = input.DateTime;
        var result = await _XqRep.UpdateAsync(xq);
        return new { code = 200, message = "更新成功", result.Entity };
    }

    //开启学期
    [HttpPost("OpenXq")]
    public async Task<dynamic> OpenXq (XqDTO input)
    {
        var allxq = await _XqRep.Where(x => true).ToListAsync();
        foreach (var x in allxq)
        {
            x.IsDeleted = false;
            await _XqRep.UpdateAsync(x);
        }
        var xq = await _XqRep.Where(x => x.Id == input.Id).FirstOrDefaultAsync();
        xq.IsDeleted = true;
        var result = await _XqRep.UpdateAsync(xq);
        return new { code = 200,message = "开启成功", result.Entity};
    }

    //关闭学期
    [HttpPost("CloseXq")]
    public async Task<dynamic> CloseXq(XqDTO input){
        var  xq = await _XqRep.Where(x => x.Id == input.Id).FirstOrDefaultAsync();
        xq.IsDeleted = false;
        var result = await _XqRep.UpdateAsync(xq);
        return new { code = 200, message = "禁用成功", result.Entity };
    }
}
