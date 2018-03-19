using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class ScoreForUpdateDto
    {
        public DateTimeOffset Timestamp { get; set; }
        public int InstanceScore { get; set; }
    }
}
