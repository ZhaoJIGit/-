using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly string _secretKey = "this_is_a_secure_key_for_jwtthis_is_a_secure_key_for_jwt"; // ȷ�������Կ�㹻�����Ұ�ȫ

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // ����ͨ��������û���֤
            if (login.Username == "testuser" && login.Password == "password")
            {
                var token = GenerateToken(login.Username);
                return Ok(new { token });
            }

            return Unauthorized();
        }

        private string GenerateToken(string username)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: null, // �����ߣ�ͨ������ָ��Ϊ null
                audience: null, // �����ߣ�ͨ������ָ��Ϊ null
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // ����ʱ��
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // �������ɵ� JWT
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
