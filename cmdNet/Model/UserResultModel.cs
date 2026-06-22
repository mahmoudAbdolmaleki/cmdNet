using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdNet.Model
{
    public class UserResultModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Status { get; set; }
        public string SimilarUserName { get; set; }

        public string SimilarDisplayName { get; set; }

        public string Action { get; set; } = "Create";
        public string NationalCode { get; set; }

        public string Mobile { get; set; }

        

    }
    public class AdUserInfo
    {
        public string UserName { get; set; }

        public string DisplayName { get; set; }


    }
}
