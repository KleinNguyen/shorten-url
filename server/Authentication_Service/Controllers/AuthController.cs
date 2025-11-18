using Authentication_Service.Data;
using Authentication_Service.DTOs;
using Authentication_Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication_Service.Services;
using BC = BCrypt.Net.BCrypt;


namespace Authentication_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AuthenticationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email
                })
                .ToListAsync();

            return Ok(users);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists");
            }
            string passwordHash = BC.HashPassword(registerDto.Password);
            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, "Account registered successfully!");
        }
        [HttpPost("login")]
        [AllowAnonymous]

        public async Task<ActionResult<TokenDto>> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại!"); 
            }

            if (!BC.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Mật khẩu không đúng!"); 
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiration = token.ValidTo,
                user = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email
                }
            });
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto updateDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.Email))
            {
                if (await _context.Users.AnyAsync(u => u.Email == updateDto.Email && u.Id != id))
                    return BadRequest("Email already in use by another account.");

                user.Email = updateDto.Email;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.UserName))
            {
                user.UserName = updateDto.UserName;
            }
            else
            {
                user.UserName = user.UserName;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Email))
            {
                user.Email = updateDto.Email;
            }
            else
            {
                user.Email = user.Email;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Account updated successfully.",
                user.Id,
                user.UserName,
                user.Email
            });
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Account deleted successfully.");
        }


        [HttpPost("send-email")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestForgotPassword(SendEmailDto sendEmailDto, EmailService emailService)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == sendEmailDto.Email);
            if (user == null)
                return BadRequest("Email not found.");

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "").Replace("/", "").Replace("=", ""); 

            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30); 
            user.ResetTokenVerified = false;
            await _context.SaveChangesAsync();

            var resetLink = $"https://shorten-url-client-7pz2.onrender.com/reset-password?token={token}&email={user.Email}";

            try
            {
                await emailService.SendEmailAsync(
                    sendEmailDto.Email,
                    "Password Reset Request",
                    $@"Hello {user.UserName},

        You requested to reset your password. Click the link below to verify your email and reset your password: {resetLink}
        This link will expire in 30 minutes."
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }

            return Ok("A password reset link has been sent to your email.");
        }

        [HttpGet("verify-token")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetToken( string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { success = false, message = "Invalid token." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token);

            if (user == null)
                return BadRequest(new { success = false, message = "Invalid token." });

            if (user.ResetTokenExpiry < DateTime.UtcNow)
                return BadRequest(new { success = false, message = "Token has expired. Please request a new password reset." });

            user.ResetTokenVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Email verified successfully. You can now reset your password.", token = token });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email);

            if (user == null)
                return BadRequest("Email not found!.");

            if (!user.ResetTokenVerified)
                return BadRequest("Email not verified. Please click the link in your email first.");

            if (user.ResetTokenExpiry < DateTime.UtcNow)
                return BadRequest("Token has expired.");

            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                return BadRequest("Passwords do not match!");

            user.PasswordHash = BC.HashPassword(resetPasswordDto.NewPassword);
            user.ResetToken = null;
            user.ResetTokenVerified = false;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Password has been reset successfully. You can now login with your new password.");
        }

    }
}

