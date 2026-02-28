using sxgl.Application.System.Dtos;
using sxgl.Core.RBAC.Entitys;
using sxgl.Core.RBAC.Entitysl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Application.System.Services;

public class KcServices : IDynamicApiController, ITransient
{
    private readonly IRepository<Kcb> _kcRep;
    private readonly IRepository<Xqb> _xqRep;
    private readonly IRepository<Xyb> _xyRep;
    private readonly IRepository<Zyb> _zyRep;

    public KcServices(IRepository<Kcb> kcRep, IRepository<Xqb> xqRep, IRepository<Xyb> xyRep, IRepository<Zyb> zyRep)
    {
        _kcRep = kcRep;
        _xqRep = xqRep;
        _xyRep = xyRep;
        _zyRep = zyRep;
    }

    //查看当前学期课程
    [HttpGet("GetNowKc")]
    public async Task<dynamic> GetNowKc() 
    {
        var xq = await _xqRep.Where(x => x.IsDeleted == true).FirstOrDefaultAsync();
        if(xq == null)
        {
            return new { code = 401, message = "请先设置当前学期" };
        }
        var kc = await _kcRep.Where(k => k.Xqid == xq.Id && k.IsDeleted == false).ToArrayAsync();
        return kc;
    }

    //添加课程
    [HttpPost("AddKc")]
    public async Task<dynamic> AddKc(KcDTO input)
    {
        var kc = new Kcb
        {
            Kcbh = input.Kcbh,
            Name = input.Name,
            Xf = input.Xf,
            Xqid = input.Xqid,
            Zyid = input.Zyid,
            Xyid = input.Xyid
        };
        var result = await _kcRep.InsertAsync(kc);
        return new {code = 200 , message = "添加成功",result.Entity};
    }

    //删除课程
    [HttpPost("DeletedKc")]
    public async Task<dynamic> DeletedKc([FromForm] int id)
    {
        var kc = await _kcRep.Where(k => k.Id == id && k.IsDeleted == false).FirstOrDefaultAsync();
       var result = await _kcRep.DeleteAsync(kc);
        return new {code = 200 ,message = "删除成功",result.Entity} ;
    }

    //修改课程信息
    [HttpPost("UpdateKc")]
    public async Task<dynamic> UpdateKc(KcDTO input)
    {
        var kc = await _kcRep.Where(k => k.Id == input.Id && k.IsDeleted == false).FirstOrDefaultAsync();
        kc.Kcbh = input.Kcbh;
        kc.Name = input.Name;
        kc.Xf = input.Xf;
        kc.Xqid = input.Xqid;
        kc.Zyid = input.Zyid;
        kc.Xyid = input.Xyid;
        var result = await _kcRep.UpdateAsync(kc);
        return new { code = 200, message = " 修改课程", result.Entity };
    }
    //根据学院id查找学院名称
    [HttpPost("GetXyById")]
    public async Task<string> GetXyById([FromForm] int id)
    {
        var xy = await _xyRep.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefaultAsync();
        return xy.Name;
    }
    //根据专业id查找专业名称
    [HttpPost("GetZyById")]
    public async Task<string> GetZyById([FromForm] int id)
    {
        var zy = await _zyRep.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefaultAsync();
        return zy.Name;
    }
    //根据学期id查找学期名
    [HttpPost("Getxqbyid")]
    public async Task<dynamic> Getxqbyid([FromForm] int id)
    {
        var xq = await _xqRep.Where(x => x.Id == id).FirstOrDefaultAsync();
        return xq.Name;
    }
}
