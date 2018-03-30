using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Library.API.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        //[Required]
        //public DateTimeOffset DateOfBirth { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Password { get; set; }

        //[MaxLength(10)]
        //public int NumberOfDevices { get; set; }

        [Required]
        [MaxLength(50)]
        public string IMEI { get; set; }

        public ICollection<Score> Scores { get; set; }
            = new List<Score>();
    }
}
