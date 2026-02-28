using sxgl.Application.System.Dtos;
using sxgl.Core.RBAC.Entitys;
using sxgl.Core.RBAC.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sxgl.Application.System.Services;

public class UserServices : IDynamicApiController, ITransient
{
    private readonly IRepository<User> _userRep;
    public UserServices(IRepository<User> userRep)
    { 
        _userRep = userRep; 
    }

    //登录
    [AllowAnonymous]
    public async Task<dynamic> Login([Required] LoginDTO input)
    {
        var password = DataEncryption.Sha1Encrypt(input.Password);
        var user = await _userRep.Where(u => u.UserName == input.Account && u.Password == password && u.IsDeleted == false).FirstAsync();
        if (user == null)
        {
            return new { Code = 401, Message = "登录失败，请重试登录" };
        }
        string token = JWTEncryption.Encrypt(new Dictionary<string, object>()
        {
            {"UserId",user.Id.ToString()},
            {"Account",user.UserName },
            {"Role",user.Role }
        }, 20);
        return new { Code = 200, Message = "登录成功", token = token };
    }


    //添加一条用户信息
    [HttpPost("AddUser")]
    public async Task<dynamic> AddUser([Required] LoginDTO input)
    {
        var user1 = await _userRep.Where(u => u.UserName == input.Account && u.IsDeleted == false).FirstOrDefaultAsync();
        if (user1 != null)
        {
            return new { code = 400, message = "该用户已存在" };
        }
        var user = new User
        {
            UserName = input.Account,
            Password = DataEncryption.Sha1Encrypt(input.Password),
            Role = input.Role
        };
        var result = await _userRep.InsertAsync(user);
        return new { code = 200, message = "用户添加成功" ,result.Entity};
    }

    //删除一条用户信息
    [HttpPost("DeleteUser")]
    public async Task<dynamic> DeleteUser([Required] LoginDTO input)
    {
        var user = await _userRep.Where(u => u.Id == input.Id && u.IsDeleted == false).FirstOrDefaultAsync();
        if (user == null)
        {
            return new { code = 404, message = "该用户不存在" };
        }
        var result = await _userRep.DeleteAsync(user);
        return new { code = 200, message = "删除成功", result.Entity };
    }

    //修改一条用户信息
    [HttpPost("UpdateUser")]
    public async Task<dynamic>Update([Required] LoginDTO input)
    {
        var user  = await _userRep.Where(u => u.Id == input.Id && u.IsDeleted == false).FirstOrDefaultAsync();
        user.Role = input.Role;
        var result =await _userRep.UpdateAsync(user);
        return new { code = 200, message ="更新成功",result.Entity };
    }
    //获取全部用户信息
    [HttpGet("GetAllUser")]
    public async Task<dynamic> GetAllUser()
    {
        var user = await _userRep.Where(x => true).ToArrayAsync();
        return user;
    }
    //修改密码
    [AllowAnonymous]
    [HttpPost("UpdatePassage")]
    public async Task<dynamic>UpdatePassage([Required] LoginDTO input)
    {
        var user = await _userRep.Where(u => u.UserName == input.Account && u.IsDeleted == false).FirstOrDefaultAsync();
        user.Password = DataEncryption.Sha1Encrypt(input.Password);
        var result = await _userRep.UpdateAsync(user);
        return new { code = 200, message = "更新成功", result.Entity };
    }

    //禁用用户
    [HttpPost("CloseUser")]
    public async Task<dynamic>CloseUser([Required] LoginDTO input)
    {
        var user = await _userRep.Where(u => u.Id == input.Id).FirstOrDefaultAsync();
        user.IsDeleted = true;
        var result = await _userRep.UpdateAsync(user);
        return new { code = 200, message = "用户禁用成功", result.Entity };
    }
    //开启用户
    [HttpPost("OpenUser")]
    public async Task<dynamic>OpenUser([Required] LoginDTO input)
    {
        var user = await _userRep.Where(u => u.Id == input.Id).FirstOrDefaultAsync();
        user.IsDeleted = false;
        var result = await _userRep.UpdateAsync(user);
        return new { code = 200, message = "用户开启成功", result.Entity };
    }
    //重置密码
    [HttpPost("RefreshPassword")]
    public async Task<dynamic>RefreshPassword([Required] LoginDTO input){
        var user = await _userRep.Where(u => u.Id == input.Id).FirstOrDefaultAsync();
        user.Password =  DataEncryption.Sha1Encrypt("123456");
        var result = await _userRep.UpdateAsync(user);
        return new { code = 200, message = "密码重置成功", result.Entity };
    }
}
