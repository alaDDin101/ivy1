using Application.Dto;
using Domain.Entities;
using Ivy.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Commons;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly IvyContext _ivyContext;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration config,
        IvyContext ivyContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _ivyContext = ivyContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (model.Password != model.ConfirmPassword)
            return BadRequest("Passwords do not match.");

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("User registered successfully.");
    }

    [HttpPost("admin-reset-password")]
    public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return NotFound("User not found.");

        var hasPassword = await _userManager.HasPasswordAsync(user);
        IdentityResult result;

        if (hasPassword)
            result = await _userManager.RemovePasswordAsync(user);
        else
            result = IdentityResult.Success;

        if (result.Succeeded)
        {
            var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
            if (addResult.Succeeded)
                return Ok("Password reset successfully.");
            else
                return BadRequest(addResult.Errors);
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
            return Unauthorized("Invalid email or password.");

        if (user.PhoneNumberConfirmed == false)
            return BadRequest(new ErrorResponse(400, "لم يتم تأكيد رقم الهاتف"));

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        // Get role
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "";

        // Get Ivy user (by MembershipUser)
        var ivyUser = await _ivyContext.Users
            .Include(u => u.PartyNavigation).ThenInclude(p => p.Person)
            .FirstOrDefaultAsync(u => u.MembershipUser == user.Id);

        if (ivyUser?.PartyNavigation?.Person == null)
            return BadRequest("User profile not found in Ivy system");

        var person = ivyUser.PartyNavigation.Person;

        var response = new LoginResponseDto
        {
            Email = user.Email!,
            Username = user.UserName!,
            FirstName = person.FirstName,
            LastName = person.LastName,
            FatherName = person.FatherName,
            NationalNumber = person.NationalNumber,
            BirthDate = person.BirthDate,
            Role = role,
            Token = "Bearer " + GenerateJwtToken(user, role)
        };

        return Ok(new { Code = 200, Message = "تم تسجيل الدخول بنجاح", Data = response });
    }

    private string GenerateJwtToken(IdentityUser user, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
