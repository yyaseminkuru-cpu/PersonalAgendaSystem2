using System.ComponentModel.DataAnnotations;

namespace PersonalAgendaSystem.Models
{
    [MetadataType(typeof(UsersMetadata))]
    public partial class Users
    {
    }

    public class UsersMetadata
    {
        [Required(ErrorMessage = "Ad soyad boş bırakılamaz.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "E-posta boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Rol seçiniz.")]
        public string Role { get; set; }
    }
}