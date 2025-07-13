using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Commons;
using Application.Dto;
using Infrastructure.Persistence;
using Domain.Entities;
using Ivy.Infrastructure.Persistence;

namespace Ivy.Controllers
{
    /// <summary>
    /// Authentication controller for user registration, login, and JWT token management
    /// </summary>
    /// <remarks>
    /// This controller handles all authentication-related operations including:
    /// - Patient registration with comprehensive personal information
    /// - User login with JWT token generation
    /// - Role-based authentication for different user types (Patient, Doctor, Clinic Staff, Admin)
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IvyContext _context;

        /// <summary>
        /// Initializes a new instance of the AuthController
        /// </summary>
        /// <param name="userManager">User manager for identity operations</param>
        /// <param name="signInManager">Sign-in manager for authentication</param>
        /// <param name="configuration">Configuration for JWT settings</param>
        /// <param name="context">Database context for entity operations</param>
        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            IvyContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Register a new patient with comprehensive personal information
        /// </summary>
        /// <remarks>
        /// Creates a complete patient profile including:
        /// - Personal information (name, birth date, address)
        /// - National number validation (11 digits)
        /// - Contact information (email, phone)
        /// - Identity user account with patient role
        /// 
        /// This endpoint automatically creates the full entity chain:
        /// Party → Person → Patient → User → IdentityUser
        /// </remarks>
        /// <param name="model">Patient registration information</param>
        /// <returns>Success response with patient details or error message</returns>
        /// <response code="200">Patient registered successfully</response>
        /// <response code="400">Invalid input data or validation errors</response>
        /// <response code="500">Internal server error during registration</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new ApiResponse<string>(400, "Email already registered."));
                }

                // Create Identity User
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new ApiResponse<string>(400, string.Join(", ", result.Errors.Select(e => e.Description))));
                }

                // Assign patient role
                await _userManager.AddToRoleAsync(user, "patient");

                // Create Party
                var party = new Party
                {
                    DispalyName = $"{model.FirstName} {model.LastName}",
                    IsActive = true
                };
                _context.Parties.Add(party);
                await _context.SaveChangesAsync();

                // Create Person
                var person = new Person
                {
                    Party = party.Id,
                    FirstName = model.FirstName,
                    FatherName = model.FatherName,
                    LastName = model.LastName,
                    MotherName = model.MotherName,
                    BirthDate = model.BirthDate,
                    Address = model.Address,
                    NationalNumber = model.NationalNumber
                };
                _context.People.Add(person);
                await _context.SaveChangesAsync();

                // Create Patient
                var patient = new Patient
                {
                    Person = person.Party,
                    PatientCode = GeneratePatientCode()
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // Create User
                var ivyUser = new Domain.Entities.User
                {
                    Party = party.Id,
                    MembershipUser = user.Id
                };
                _context.Users.Add(ivyUser);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new ApiResponse<string>(200, "Patient registered successfully.", 
                    $"Patient Code: {patient.PatientCode}"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<string>(500, "An error occurred during registration.", ex.Message));
            }
        }

        /// <summary>
        /// Authenticate user and generate JWT token
        /// </summary>
        /// <remarks>
        /// Authenticates users with email and password, then generates a JWT token containing:
        /// - User ID and email
        /// - Assigned roles (patient, doctor, clinic-staff, admin)
        /// - Token expiration (configurable)
        /// 
        /// The token should be included in subsequent requests using the Authorization header:
        /// Authorization: Bearer {token}
        /// </remarks>
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token and user information or error message</returns>
        /// <response code="200">Login successful with JWT token</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="500">Internal server error during authentication</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return BadRequest(new ApiResponse<string>(400, "Invalid email or password."));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles);

                var response = new LoginResponseDto
                {
                    Token = token,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                };

                return Ok(new ApiResponse<LoginResponseDto>(200, "Login successful.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, "An error occurred during login.", ex.Message));
            }
        }

        /// <summary>
        /// Generate a unique patient code for registration
        /// </summary>
        /// <returns>Unique patient code</returns>
        private string GeneratePatientCode()
        {
            return $"P{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        /// <param name="user">Identity user</param>
        /// <param name="roles">User roles</param>
        /// <returns>JWT token string</returns>
        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
