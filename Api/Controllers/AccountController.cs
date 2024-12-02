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
using Microsoft.Extensions.Hosting;
using MimeKit;
using System.Web;
using Api.Services;
using FluentEmail.Core;
using System.Net;


namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly IConfiguration _configuration;
        
        private readonly Microsoft.Extensions.Hosting.IHostingEnvironment _env;

        private readonly IEmailService _emailService;



        public AccountController(UserManager<AppUser> userManager, IConfiguration configuration, Microsoft.Extensions.Hosting.IHostingEnvironment env, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _env=env;
            _emailService = emailService;
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

        [HttpGet("get-user-details")]
        public async Task<ActionResult<UserDetailResponseDto>> GetUserDetails()
        {
            var currentUserId=User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user=await _userManager.FindByIdAsync(currentUserId);

            if(user is null)
            {
                return NotFound();
            }

            var response = new UserDetailResponseDto
            {
                Id = user.Id,
                Name=user.FullName,
                Email=user.Email,
                avatar=user.Avatar,
                joinDate=user.CreatedDate,
                lastActive=user.LastActiveDate,
                projects = ["Fat Boy","City","robot"],
                skills = ["3d","Auto CAD"],
                Role="Admin",
                status="active"
            };
            return Ok(response);

        }

        [HttpPatch("user")]
        public async Task<ActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);

            if (user is null)
            {
                return NotFound();
            }

            if (request.Name !=null)
            {
                user.FullName = request.Name;   
            }

            if (request.Email != null)
            {
                user.Email = request.Email;
            }

            if (request.Avatar != null)
            {
                user.Avatar = request.Avatar;
            }

            await _userManager.UpdateAsync(user);

            return Ok();

        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult<AuthResponseDto>> ForgotPassword( ForgotPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.email);
            if (user is null)
            {
                return NotFound();
            }

            var token=await _userManager.GeneratePasswordResetTokenAsync(user);

            var pathToFile=_env.ContentRootPath+Path.DirectorySeparatorChar.ToString()
                +"Templates"
                + Path.DirectorySeparatorChar.ToString()
                +"Email"
                + Path.DirectorySeparatorChar.ToString()
                + "Forgotpassword.html"
                ;
            var builder=new BodyBuilder();
            using(StreamReader Sourcereader = System.IO.File.OpenText(pathToFile))
            {
                builder.HtmlBody=Sourcereader.ReadToEnd();   
            }
            var forntEndAddress = _configuration.GetSection("FrontEnd").GetSection("Address").Value!;
            var frontEndPageName= _configuration.GetSection("FrontEnd").GetSection("reset-password").Value!;
            var resetLink = $"{forntEndAddress}{frontEndPageName}?email={user.Email}&token={WebUtility.UrlEncode(token)}";
            string messageBody = string.Format(builder.HtmlBody, resetLink, WebUtility.UrlEncode(resetLink));
            Console.WriteLine(token);

            var subject = "Email sent with password reset link. Please check you mail.";

            EmailMessageModel emailMessage = new(
                request.email,
                subject,
                messageBody
            );

            await _emailService.Send(emailMessage);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Email sent with password reset link. Please check."
            });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponseDto>> ResetPassword(ResetPasswordRequestDto request)
        {
            Console.WriteLine(request.Token);
            var user=await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false,
                Message="User does not exist."
                });   
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password reset successfully"
                });
            }

            return BadRequest(new AuthResponseDto {
            IsSuccess= false,
            Message=result.Errors.FirstOrDefault()!.Description
            });

        }

        [HttpPost("change-password")]
        public async Task<ActionResult<AuthResponseDto>> ChangePassword(ChangePasswordRequestDto request)
        {
          
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist."
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Change Password Successfully"
                });
            }

            return BadRequest(new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()!.Description
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
