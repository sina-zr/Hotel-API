using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelAPI.Models;

namespace EFDataAccessLibrary.Models
{
    public class Guest
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string FirstName { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string LastName { get; set; }

        [Required]
        [Column(TypeName = "nchar(50)")]
        public string EmailAddress { get; set; }

        // Add the User property
        public ApplicationUser User { get; set; }
    }
}
