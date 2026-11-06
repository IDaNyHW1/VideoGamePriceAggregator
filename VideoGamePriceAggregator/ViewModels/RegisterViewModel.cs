using System.ComponentModel.DataAnnotations;

namespace VideoGamePriceAggregator.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен содержать от 3 до 50 символов")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}