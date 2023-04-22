using ClientApp.Data;
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
        private readonly SignInManager<AccountModel> _signInManager;
        private readonly UserManager<AccountModel> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _email;
        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public MAUIApiController(IDbContextFactory<AppDbContext> dbFactory, SignInManager<AccountModel> signInManager, UserManager<AccountModel> userManager, IConfiguration configuration, IEmailService email)
        {
            _dbFactory = dbFactory;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _email = email;
        }
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(PasswordResetViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var rs = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if(rs.Succeeded)
            {
                return Ok(user);
            }
            return BadRequest();
        }

        [AllowAnonymous]
        [HttpGet("ResetPassword/{email}")]
        public async Task<IActionResult> ResetPassword(string email)
        {
            string generatedPassword = RandomString(8);
            var user = await _userManager.FindByEmailAsync(email);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, generatedPassword);
            string body = $"Your new password is: {generatedPassword}";
            await _email.SendEmail(email, body);
            return Ok();
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
                    var userIdentity = await _userManager.FindByNameAsync(username);
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
            string password = user.Password;
            user.UserName = user.Email;
            user.Password = null;
            var userIdentityResult = await _userManager.CreateAsync(user, password);
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
        [Authorize]
        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser(AccountModel account)
        {
            var user = await _userManager.FindByNameAsync(account.UserName);
            user.FirstName = account.FirstName;
            user.LastName = account.LastName;
            var result = await _userManager.UpdateAsync(user);
            if(result.Succeeded)
            {
                return Ok(user);
            }
            return BadRequest();
        }

        [NonAction]
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<string> GenerateJSONWebToken(AccountModel userIdentity)
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
