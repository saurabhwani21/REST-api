using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{


    // This class is to map the input HTTP Post request according to the backend. 
    public class UserForCreationDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IMEI { get; set; }

        public ICollection<ScoreForCreationDto> Scores { get; set; }
        = new List<ScoreForCreationDto>();
    }
}
