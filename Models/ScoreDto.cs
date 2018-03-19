using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class ScoreDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int InstanceScore { get; set; }
        public Guid UserId { get; set; }
    }
}
