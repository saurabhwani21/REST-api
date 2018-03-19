using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class ScoreForCreationDto
    {
        //[Required]
        public DateTimeOffset Timestamp { get; set; }

        //[Required]
        //[MaxLength(10)]
        public int InstanceScore { get; set; }
    }
}
