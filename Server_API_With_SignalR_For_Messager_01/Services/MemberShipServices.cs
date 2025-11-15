using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using demo_158.Services.Enums;
using Microsoft.EntityFrameworkCore;
using WebSocketSharpServer.DbContext.DbModel;
using WebSocketSharpServer.DbContext.Entities;
using WebSocketSharpServer.Models;

namespace WebSocketSharpServer.Services
{
    public class MemberShipServices(ApplicationDbModel dbContext)
    {
    public async Task<bool> IsUserExistAsync(string username)
    {
        return await dbContext.Users.AnyAsync(user => user.Username == username);
    }

    public async Task<User> GetUserAsync(string username)
    {
        var user = await dbContext.Users.Include(e=>e.Image).FirstOrDefaultAsync(t => t.Username == username);
        if (user == null)
        {
            return null;
        }
        return user;
        
    }

    public async Task GetUserImageAsync(User? user)
    {
        if (user is null)
        {
            return;
        }
        var entry = dbContext.Users.Entry(user);
        await entry.Reference(e => e.Image).LoadAsync();

    }
    public async Task<User> GetUserAsync(int userId)
    {
        var user = await dbContext.Users.Include(e => e.Image).FirstOrDefaultAsync(e => e.Id == userId);
        if (user == null)
        {
            return null;
        }
        return user;
        }
    public  UserModelFromServer ConvertUserToUserModelFromServer(User user)
    {

        return new UserModelFromServer()
        {
            Username = user.Username,
            BioCaption = user.BioCaption,
            Email = user.Email,
        };
    }

    public async Task<ServerAnswer> UploadProfileImage(byte[] imageBytes, int userId)
    {
        if (!await dbContext.Users.AnyAsync(e=>e.Id == userId))
        {
            return ServerAnswer.bad;
        }

        var user = await GetUserAsync(userId);
        UserImage image;
            
        if (user.Image == null)
        {
            image = new UserImage();
            image.UserId = userId;
            user.Image = image;

        }
        else
        {
            image = user.Image;
        }
        image.ImageData = imageBytes;
            await dbContext.SaveChangesAsync();
        return ServerAnswer.ok;
            
    }
    public async Task<int> CreateUserAsync(string email, string username, string password)
    {
        string passowrdSalt = Guid.NewGuid().ToString("N");
        var user = new User()
        {
            Email = email,
           
            Username = username,
            Password = HashedPassword(password, passowrdSalt),
            PasswordSalt = passowrdSalt,
            RegisterDate = DateTime.Now
        };
        dbContext.Add(user);
        await dbContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> UsernamePasswordValidationAsync(string username, string password)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Username == username);
        if (user == null)
            return false;

        return user.Password == HashedPassword(password, user.PasswordSalt);

    }

    public string HashedPassword(string password, string passwordSalt)
    {

        var sha256 = SHA256.Create();

        var hashedpassword =
            Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF32.GetBytes(password + passwordSalt)));
        return hashedpassword;
    }

    }


}
