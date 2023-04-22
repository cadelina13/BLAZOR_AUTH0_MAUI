using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class AccountModel : IdentityUser
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Picture { get; set; }
        public DateTime DateCreated { get; set; }
        public AccountModel()
        {
            DateCreated = DateTime.Now;
        }
    }
}
