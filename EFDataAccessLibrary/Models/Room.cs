using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFDataAccessLibrary.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        public int RoomNumber { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int RoomTypeId { get; set; }
    }
}
