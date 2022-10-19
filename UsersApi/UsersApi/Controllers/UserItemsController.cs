using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Cryptography;
using UsersApi.Models;

namespace UsersApi.Controllers
{
    [Route("api/UserItems")]
    [ApiController]
    public class UserItemsController : ControllerBase
    {
        private readonly UserContext _context;

        public UserItemsController(UserContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<UserResponce>> GetUsers()
        {
            if (_context.UserItems == null)
                return NotFound();
            UserResponce responce = new UserResponce();
            var temp = await _context.UserItems.Select(x => ItemToDTO(x)).ToListAsync();
            responce.UserItems = temp;
            return responce;
        }

        [HttpGet("{login}")]
        public async Task<ActionResult<UserItemDTO>> GetUser(string login)
        {
            if (_context.UserItems == null)
                return NotFound();
            var user = await _context.UserItems.FindAsync(login);
            if (user == null)
                return new UserItemDTO();
            return ItemToDTO(user);
        }

        [HttpPut("{login}/nickname")]
        public async Task<IActionResult> UpdateNickname(string login, string nickname)
        {
            var user = await _context.UserItems.FindAsync(login);
            if (user == null)
                return NotFound();
            user.Nickname = nickname;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!UserExists(login))
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPut("{login}/password")]
        public async Task<IActionResult> UpdatePassword(string login, string password)
        {
            var user = await _context.UserItems.FindAsync(login);
            if (user == null)
                return NotFound();
            user.Password = HashPassword(user.Login + password); ;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!UserExists(login))
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPost("Create")]
        public async Task<ActionResult<UserItem>> CreateUser(UserItem user)
        {
            if (user.Login == null)
                return NotFound();
            if (UserExists(user.Login))
                return Conflict();
            user.Password = HashPassword(user.Login + user.Password);
            _context.UserItems.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { login = user.Login }, ItemToDTO(user));
        }

        [HttpPost("Login")]
        public async Task<ActionResult<bool>> TryLogin(string login, string password)
        {
            if (_context.UserItems == null)
                return false;
            var user = await _context.UserItems.FindAsync(login);
            if (user == null || user.Password== null)
                return false;
            return VerifyHashedPassword(user.Password, user.Login + password);
        }

        [HttpDelete("{login}")]
        public async Task<IActionResult> DeleteUser(string login)
        {
            if (_context.UserItems == null)
            {
                return NotFound();
            }
            var todoItem = await _context.UserItems.FindAsync(login);
            if (todoItem == null)
            {
                return NotFound();
            }
            _context.UserItems.Remove(todoItem);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool UserExists(string login)
        {
            return _context.UserItems.Any(e => e.Login == login);
        }

        private static UserItemDTO ItemToDTO(UserItem todoItem) => new UserItemDTO
        {
            Login = todoItem.Login,
            Nickname = todoItem.Nickname,
            Money = todoItem.Money,
            Record = todoItem.Record
        };

        private static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        private static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }

        private static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
    }
}
