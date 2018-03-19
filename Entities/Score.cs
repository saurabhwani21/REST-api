using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.API.Entities
{
    public class Score
    {
        [Key]
        public Guid Id { get; set; }
        //public int Id { get; set; }


        //[Required]
        //[MaxLength(100)]
        //public string IMEI { get; set; }

        //[Required]
        //[MaxLength(100)]
        //public string IMEI { get; set; }

        [Required]
        public DateTimeOffset TimeStamp { get; set; }

        [Required]
        [MaxLength(10)]
        public int InstanceScore { get; set; }

        //[MaxLength(500)]
        //public string Description { get; set; }

        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        //public User User { get; set; }


    }
}
