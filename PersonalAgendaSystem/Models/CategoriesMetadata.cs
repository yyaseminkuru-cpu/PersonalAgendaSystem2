using System.ComponentModel.DataAnnotations;

namespace PersonalAgendaSystem.Models
{
    [MetadataType(typeof(CategoriesMetadata))]
    public partial class Categories
    {
    }

    public class CategoriesMetadata
    {
        [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
        public string CategoryName { get; set; }
    }
}