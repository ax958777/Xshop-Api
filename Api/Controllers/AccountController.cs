using Api.Dto;
using Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly IConfiguration _configuration;

        public AccountController(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> login(LoginRequestDto loginDto)
        {
           if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user == null)
            {
                return Unauthorized(
                    new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message ="User Not Found"
                    }
                    );
            };

            var result=await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return Unauthorized(
                    new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Password not matched."
                    }
                    );
            };

            var token = GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                IsSuccess=true,
                Message="Login Success",
                Token = token
            });

        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto registerDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser
            {
                Email = registerDto.Email,
                FullName = registerDto.Name,
                UserName = registerDto.Email
            };

            var result=await _userManager.CreateAsync(user,registerDto.Password);

            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Account Created Succesfully!"
            });


        }

        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings").GetSection("securityKey").Value!);

            var roles=_userManager.GetRolesAsync(user).Result;

            List<Claim> claims = [
                new( JwtRegisteredClaimNames.Email,user.Email??""),
                new( JwtRegisteredClaimNames.Name,user.FullName??""),
                new( JwtRegisteredClaimNames.NameId,user.Id??""),
                new( JwtRegisteredClaimNames.Aud,_configuration.GetSection("JwtSettings").GetSection("ValidAudience").Value!),
                new( JwtRegisteredClaimNames.Iss,_configuration.GetSection("JwtSettings").GetSection("ValidIssuer").Value!),
                ];

            foreach (var role in roles) { 
            claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)

            };

            var token=tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);  
        }
    }
}
