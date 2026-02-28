using sxgl.Application.System.Dtos;
using sxgl.Core.RBAC.Entitys;
using sxgl.Core.RBAC.Entitysl;
using sxgl.Core.RBAC.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Application.System.Services;

public class TeaServices : IDynamicApiController, ITransient 
{
    private readonly IRepository<Teab> _teaRep;
    private readonly IRepository<User> _userRep;
    private readonly IRepository<Xyb> _xyRep;
    private readonly IRepository<Zyb> _zyRep;

    public TeaServices(IRepository<Teab> teaRep, IRepository<User> userRep , IRepository<Xyb> xyRep, IRepository<Zyb> zyRep)
    {
        _teaRep = teaRep;
        _userRep = userRep;
        _xyRep = xyRep;
        _zyRep = zyRep;
    }

    //查看所有教师
    [HttpGet("GetAllTea")]
    public async Task<dynamic> GetAllTea()
    {
        var tea = await _teaRep.Where(t => t.IsDeleted ==false).ToArrayAsync();
        return tea;
    }
    //添加教师信息
    [HttpPost("AddTea")]
    public async Task<dynamic> AddTea(TeaDTO input)
    {
        var tea = new Teab
        {
            Gh = input.Gh,
            Name = input.Name,
            Phone = input.Phone,
            Xyid = input.Xyid,
            Zyid = input.Zyid
        };
        var result = await _teaRep.InsertAsync(tea);
        var user = new User
        {
            UserName = input.Gh,
            Password = DataEncryption.Sha1Encrypt("123456"),
            Role = "Teacher"
        };
        await _userRep.InsertAsync(user);
        return new {code = 200 , message = "添加成功",result.Entity };
    }

    //删除教师信息
    [HttpPost("DeletedTea")]
    public async Task<dynamic> DeletedTea([FromForm] int id)
    {
        var tea = await _teaRep.Where(t => t.Id == id && t.IsDeleted == false).FirstOrDefaultAsync();
        var user = await _userRep.Where(u => u.UserName == tea.Gh && u.IsDeleted == false).FirstOrDefaultAsync();
        await _userRep.DeleteAsync(user);
        var result = await _teaRep.DeleteAsync(tea);
        return new { code = 200, message = "删除成功", result.Entity };
    }

    //修改教师信息
    [HttpPost("UpdateTea")]
    public async Task<dynamic> UpdateTea(TeaDTO input)
    {
        var tea = await _teaRep.Where(t => t.Id == input.Id && t.IsDeleted == false).FirstOrDefaultAsync();
        tea.Name = input.Name;
        tea.Phone= input.Phone;
        tea.Xyid = input.Xyid;
        tea.Zyid = input.Zyid;

        var result = await _teaRep.UpdateAsync(tea);
        return new { code = 200, message = "更新成功",result.Entity };
    }

    //根据学院id查找学院名称
    [HttpPost("GetXyById")]
    public async Task<string> GetXyById([FromForm] int id)
    {
        var xy = await _xyRep.Where(x => x.Id == id && x.IsDeleted ==false).FirstOrDefaultAsync();
        return xy.Name;
    }
    //根据专业id查找专业名称
    [HttpPost("GetZyById")]
    public async Task<string> GetZyById([FromForm] int id)
    {
        var zy = await _zyRep.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefaultAsync();
        return zy.Name;
    }
}
