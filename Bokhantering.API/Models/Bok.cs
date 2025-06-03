using System.ComponentModel.DataAnnotations;

namespace Bokhantering.API.Models
{
    public class Bok
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public required string Title { get; set; }

        [Required]
        [MaxLength(150)]
        public required string Author { get; set; }

        [Required]
        public required DateTime PublicationDate { get; set; }
    }
}
