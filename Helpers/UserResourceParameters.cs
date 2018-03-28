using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class UserResourceParameters
    {
        const int maxPageSize = 20;

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string IMEI { get; set; }

        public string SearchQuery { get; set; }

        public string OrderBy { get; set; } = "Name";

        // To store the fields of data needed by the user. 
        public string Fields { get; set; }
    }
}
