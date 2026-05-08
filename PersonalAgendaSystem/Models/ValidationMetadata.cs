using System;
using System.ComponentModel.DataAnnotations;

namespace PersonalAgendaSystem.Models
{
    [MetadataType(typeof(UsersMetadata))]
    public partial class Users
    {
    }

    public class UsersMetadata
    {
        [Required(ErrorMessage = "Ad soyad bos birakilamaz.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Kullanici adi bos birakilamaz.")]
        [StringLength(50, ErrorMessage = "Kullanici adi en fazla 50 karakter olabilir.")]
        [Display(Name = "Kullanici Adi")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Sifre bos birakilamaz.")]
        [StringLength(50, ErrorMessage = "Sifre en fazla 50 karakter olabilir.")]
        [Display(Name = "Sifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "E-posta bos birakilamaz.")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Rol seciniz.")]
        [Display(Name = "Rol")]
        public string Role { get; set; }
    }

    [MetadataType(typeof(AgendaItemsMetadata))]
    public partial class AgendaItems
    {
    }

    public class AgendaItemsMetadata
    {
        [Required(ErrorMessage = "Baslik bos birakilamaz.")]
        [StringLength(100, ErrorMessage = "Baslik en fazla 100 karakter olabilir.")]
        [Display(Name = "Baslik")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Aciklama bos birakilamaz.")]
        [StringLength(500, ErrorMessage = "Aciklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Aciklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Baslangic tarihi seciniz.")]
        [Display(Name = "Baslangic Tarihi")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitis tarihi seciniz.")]
        [Display(Name = "Bitis Tarihi")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Oncelik seciniz.")]
        [Display(Name = "Oncelik")]
        public string Priority { get; set; }

        [Required(ErrorMessage = "Durum seciniz.")]
        [Display(Name = "Durum")]
        public string Status { get; set; }
    }

    [MetadataType(typeof(CategoriesMetadata))]
    public partial class Categories
    {
    }

    public class CategoriesMetadata
    {
        [Required(ErrorMessage = "Kategori adi bos birakilamaz.")]
        [StringLength(100, ErrorMessage = "Kategori adi en fazla 100 karakter olabilir.")]
        [Display(Name = "Kategori")]
        public string CategoryName { get; set; }
    }
}
