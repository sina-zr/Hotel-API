using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFDataAccessLibrary.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int GuestId { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int RoomId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool CheckedIn { get; set; }

        [Required]
        [Column(TypeName = "money")]
        public decimal TotalCost { get; set; }
    }
}
