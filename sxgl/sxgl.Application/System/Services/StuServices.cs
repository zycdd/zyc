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

public class StuServices : IDynamicApiController ,ITransient
{
    private readonly IRepository<Stub> _stuRep;
    private readonly IRepository<User> _userRep;
    private readonly IRepository<Xyb> _xyRep;
    private readonly IRepository<Zyb> _zyRep;
    public StuServices(IRepository<Stub> stuRep, IRepository<User> userRep, IRepository<Xyb> xyRep, IRepository<Zyb> zyRep)
    {
        _stuRep = stuRep;
        _userRep = userRep;
        _xyRep = xyRep;
        _zyRep = zyRep;
    }

    //查看所有学生信息
    [HttpGet("GetAllStu")]
    public async Task<dynamic> GetAllStu()
    {
        var stu  = await _stuRep.Where(s => s.IsDeleted == false).ToArrayAsync();
        return stu;
    }

    //增加学生
    [HttpPost("AddStu")]
    public async Task<dynamic> AddStu(StuDTO input)
    {
        var stu = new Stub
        {
            Xh = input.Xh,
            Name = input.Name,
            Phone = input.Phone,
            Xyid = input.Xyid,
            Zyid = input.Zyid,
            Sflx = input.Sflx
        };
        var result = await _stuRep.InsertAsync(stu);
        var user = new User
        {
            UserName = input.Xh,
            Password = DataEncryption.Sha1Encrypt("123456"),
            Role = "Student"
        };
        await _userRep.InsertAsync(user);
        return new { code = 200, message = "添加成功", result.Entity };
    }
    //删除学生信息
    [HttpPost("DeletedStu")]
    public async Task<dynamic> DeletedStu([FromForm] int id)
    {
        var stu = await _stuRep.Where(s => s.Id == id && s.IsDeleted == false).FirstOrDefaultAsync();
        var user = await _userRep.Where(u => u.UserName == stu.Xh && u.IsDeleted == false).FirstOrDefaultAsync();
        await _userRep.DeleteAsync(user);
        var result = await _stuRep.DeleteAsync(stu);
        return new { code = 200, message = "删除成功", result.Entity };
    }
    //修改学生信息
    [HttpPost("UpdateStu")]
    public async Task<dynamic> UpdateStu(StuDTO input)
    {
        var stu = await _stuRep.Where(t => t.Id == input.Id && t.IsDeleted == false).FirstOrDefaultAsync();

        stu.Name = input.Name;
        stu.Phone = input.Phone;
        stu.Xyid = input.Xyid;
        stu.Zyid = input.Zyid;
        stu.Sflx = input.Sflx;

        var result = await _stuRep.UpdateAsync(stu);
        return new { code = 200, message = "更新成功", result.Entity };
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
}
