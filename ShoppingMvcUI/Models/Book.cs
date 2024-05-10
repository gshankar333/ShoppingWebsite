using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMvcUI.Models
{
    [Table("Book")]
    public class Book
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(55)]

        public string? BookName { get; set; }

        [Required]
        [MaxLength(40)]

        public string? AuthorName { get; set; }

        [Required]
        public string? Image { get; set; }
        public double Price { get; set; }
        public int GenreId { get; set; }

        public Genre Genre { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }
        public List<CartDetail> CartDetail { get; set; }

        [NotMapped]
        public string GenreName { get; set; }


    }
}
