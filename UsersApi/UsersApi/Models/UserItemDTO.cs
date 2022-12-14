using System.ComponentModel.DataAnnotations;

namespace UsersApi.Models
{
    public class UserItemDTO
    {
        [Key]
        public string? Login { get; set; }
        public string? Nickname { get; set; }
        public decimal Money { get; set; }
        public long Record { get; set; }
    }
}
