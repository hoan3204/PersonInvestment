using System.ComponentModel.DataAnnotations;

namespace PersonalInvestmentSystem.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ho va ten khong duoc bo trong")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email khong duoc bo trong")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mat khau khong duoc bo trong")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mat khau xac nhan khong khop")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name ="Ngày sinh")]
        public DateTime DateOfBirth { get; set; }
    }
}
