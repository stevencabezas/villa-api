using System.ComponentModel.DataAnnotations;

namespace Villa_API.Models.Dto
{
    public class NumeroVillaCreateDto
    {
        [Required]
        public int VillaNo { get; set; }

        [Required]
        public int VillaId { get; set; }

        public string Detalle { get; set; }
    }
}
