using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ServerApi.Data;
using SharedLibrary.Models;
using System.Security.Claims;

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
                var userExist = db.Accounts.Any(x => x.Id == account.Id);
                db.Accounts.Update(account);
                db.SaveChanges();
                return Ok(account);
            }
        }
        [HttpPost("SaveUser")]
        public IActionResult SaveUser(AccountModel account)
        {
            using (var db = _dbFactory.CreateDbContext())
            {
                var userId = User.Claims.Where(c => c.Type.Equals("sub"))
                    .Select(c => c.Value)
                    .FirstOrDefault() ?? string.Empty;
                var userExist = db.Accounts.Any(x => x.Id == account.Id);
                if(!userExist)
                {
                    db.Accounts.Add(account);
                    db.SaveChanges();
                }
                return Ok(account);
            }
        }
        [HttpGet("GetUser/{userId}")]
        public IActionResult GetUser(string userId)
        {
            var userId2 = User.Claims.Where(c => c.Type.Equals("sub"))
                    .Select(c => c.Value)
                    .FirstOrDefault() ?? string.Empty;
            using (var db = _dbFactory.CreateDbContext())
            {
                var user = db.Accounts.Where(x => x.Id == userId).FirstOrDefault();
                return Ok(user);
            }
        }
    }
}
