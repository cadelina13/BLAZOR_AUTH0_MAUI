using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ServerApi.Data;
using SharedLibrary.Models;

namespace ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MAUIApiController : ControllerBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public MAUIApiController(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }
        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser(AccountModel account)
        {
            using (var db = _dbFactory.CreateDbContext())
            {
                var user = db.Accounts.Where(x=>x.Id == account.Id).FirstOrDefault();
                if(user == null)
                {
                    db.Accounts.Add(account);
                }
                else
                {
                    db.Accounts.Update(account);
                }
            }
            return default;
        }
        [HttpGet("GetUser/{username}")]
        public IActionResult GetUser(string username)
        {
            using (var db = _dbFactory.CreateDbContext())
            {
                var user = db.Accounts.Where(x => x.Id == username).FirstOrDefault();
                return user != null ? Ok(user) : NotFound();
            }
        }
    }
}
