using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using ServerApi.Data;
using SharedLibrary.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServerApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MAUIApiController : ControllerBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public MAUIApiController(IDbContextFactory<AppDbContext> dbFactory, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _dbFactory = dbFactory;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser(AccountModel account)
        {
            using (var db = _dbFactory.CreateDbContext())
            {
                string username = account.Email;
                string password = account.Password;
                Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(username, password, false, false);
                if (signInResult.Succeeded)
                {
                    IdentityUser userIdentity = await _userManager.FindByNameAsync(username);
                    string JSONWebTokenAsString = await GenerateJSONWebToken(userIdentity);
                    return Ok(JSONWebTokenAsString);
                }
                return Unauthorized(account);
            }
        }
        [AllowAnonymous]
        [Route("RegisterUser")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] AccountModel user)
        {
            string username = user.Email;
            string password = user.Password;
            IdentityUser userIdentity = new IdentityUser
            {
                Email = username,
                UserName = username
            };
            var userIdentityResult = await _userManager.CreateAsync(userIdentity, password);
            if (userIdentityResult.Succeeded)
            {
                return Ok(new { userIdentityResult.Succeeded });
            }
            string errorsToReturn = "Registration failed with the following errors";
            foreach (var err in userIdentityResult.Errors)
            {
                errorsToReturn += Environment.NewLine;
                errorsToReturn += $"Error code: {err.Code} - {err.Description}";
            }
            return StatusCode(StatusCodes.Status500InternalServerError, errorsToReturn);
        }
        [HttpGet("GetUser/{email}")]
        public async Task<IActionResult> GetUser(string email)
        {
            var userIdentity = await _userManager.FindByNameAsync(email);
            return Ok(userIdentity);
        }

        [NonAction]
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<string> GenerateJSONWebToken(IdentityUser userIdentity)
        {
            SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            SigningCredentials credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            // Claim = the person trying to sign in claiming to be
            var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, userIdentity.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userIdentity.Id),
        };
            var roleNames = await _userManager.GetRolesAsync(userIdentity);
            claims.AddRange(roleNames.Select(roleName => new Claim(ClaimsIdentity.DefaultNameClaimType, roleName)));
            JwtSecurityToken token = new JwtSecurityToken
                (
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Issuer"],
                    claims,
                    null,
                    expires: DateTime.UtcNow.AddDays(28),
                    credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
