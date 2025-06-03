using System.ComponentModel.DataAnnotations;

namespace BlockShare.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // буде зберігатись хеш пароля

        public string WalletAddress { get; set; }

		public bool EmailConfirmed { get; set; }   // нове поле
		public string EmailConfirmationToken { get; set; } // для підтвердження
	}
}
