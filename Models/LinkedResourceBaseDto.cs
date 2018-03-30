using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class LinkedResourceBaseDto
    {

        public List<LinkDto> Links { get; set; }
        = new List<LinkDto>();
    }
}
