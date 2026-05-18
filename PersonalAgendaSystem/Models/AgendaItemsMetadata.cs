using System;
using System.ComponentModel.DataAnnotations;

namespace PersonalAgendaSystem.Models
{
    [MetadataType(typeof(AgendaItemsMetadata))]
    public partial class AgendaItems
    {
    }

    public class AgendaItemsMetadata
    {
        [Required(ErrorMessage = "Başlık boş bırakılamaz.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama boş bırakılamaz.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi seçiniz.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi seçiniz.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Öncelik seçiniz.")]
        public string Priority { get; set; }

        [Required(ErrorMessage = "Durum seçiniz.")]
        public string Status { get; set; }
    }
}