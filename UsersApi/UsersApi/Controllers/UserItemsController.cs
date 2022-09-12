using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var todoItem = await _context.UserItems.FindAsync(login);
            if (todoItem == null)
                return NotFound();
            return ItemToDTO(todoItem);
        }

        [HttpPut("{login}")]
        public async Task<IActionResult> UpdateUser(string login, UserItemDTO todoItemDTO)
        {
            if (login != todoItemDTO.Login)
                return BadRequest();
            var todoItem = await _context.UserItems.FindAsync(login);
            if (todoItem == null)
                return NotFound();
            todoItem.Nickname = todoItemDTO.Nickname;
            todoItem.Login = todoItemDTO.Login;
            todoItem.Password = todoItemDTO.Password;
            todoItem.Money = todoItemDTO.Money;
            todoItem.Record = todoItemDTO.Record;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!UserExists(login))
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<UserItem>> CreateUser(UserItemDTO todoItemDTO)
        {
            var todoItem = new UserItem
            {
                Nickname = todoItemDTO.Nickname,
                Login = todoItemDTO.Login,
                Password = todoItemDTO.Password,
                Money = todoItemDTO.Money,
                Record = todoItemDTO.Record
            };
            _context.UserItems.Add(todoItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { login = todoItem.Login }, ItemToDTO(todoItem));
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
            return NoContent();
        }

        private bool UserExists(string login)
        {
            return _context.UserItems.Any(e => e.Login == login);
        }

        private static UserItemDTO ItemToDTO(UserItem todoItem) => new UserItemDTO
        {
            Login = todoItem.Login,
            Nickname = todoItem.Nickname,
            Password = todoItem.Password,
            Money = todoItem.Money,
            Record = todoItem.Record
        };
    }
}
