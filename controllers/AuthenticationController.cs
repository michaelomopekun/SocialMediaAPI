using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NanoidDotNet;
using SocialMediaAPI.Constants;
using SocialMediaAPI.Models.Domain.User;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Numbers = "0123456789";
    private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    private const string NanoidSize = UppercaseLetters + LowercaseLetters + Numbers;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IEmailService _emailService;

    public AuthenticationController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthenticationController> logger, IEmailService emailService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
        _emailService = emailService;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] Register register)
    {
        try
        {
            var userExistsByEmail = await _userManager.FindByEmailAsync(register.Email);
            if(userExistsByEmail != null)
            {
                return BadRequest(new {Status = "Error", message = "Email already registered"});
            }

            var userExistsByUserName = await _userManager.FindByNameAsync(register.UserName);
            if(userExistsByUserName != null)
            {
                return BadRequest(new {Status = "Error", message = "Username already registered"});
            }

            if(register.Password != register.ConfirmPassword)
            {
                return BadRequest(new {Status = "Error", message = "Passwords do not match"});
            }

            if(!(register.Password.Contains(UppercaseLetters) || register.Password.Contains(LowercaseLetters) || register.Password.Contains(Numbers) || register.Password.Contains(SpecialCharacters) || register.Password.Length >= 8))
            {
                return BadRequest(new {Status = "Error", message = "Password must contain at least one uppercase letter, one lowercase letter, one number, one special character and 8 characters long"});
            }

            var user = new ApplicationUser()
            {
                Id = Nanoid.Generate(NanoidSize, 7),
                FirstName = register.FirstName,
                LastName = register.LastName,
                UserName = register.UserName,
                Email = register.Email,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            };

            var result = await _userManager.CreateAsync(user, register.Password);
            await _userManager.AddToRoleAsync(user, UserRoles.User);

            if(!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "User Creation Failed! Please check user details and try again.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            if(token == null)
            {
                return BadRequest(new {Status = "Error", message = "Error generating email confirmation token"});
            }

            var confirmationLink = Url.Action("ConfirmEmail", "Authentication", new { userId = user.Id, Token = token }, Request.Scheme);
            if(confirmationLink == null)
            {
                return BadRequest(new {Status = "Error", message = "Error generating email confirmation link"});
            }

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Confirm your email => SocialMediaApi", $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>click here</a>");
            }
            catch(Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError("{Error}: Error sending email to user: {UserName}", errorMessage, register.UserName);
                return StatusCode(500, new {Status = "Error", message = "REGISTER :endpoint: Error sending email", errorMessage});
            }

            _logger.LogInformation("User created: {UserName}", register.UserName);

            _logger.LogInformation("Confirmation link: {ConfirmationLink}", confirmationLink);
            
            return Ok(new {Status = "Success", message = "User created successfully. Please check your email to confirm your account."});
        }
        catch(Exception ex)
        {
            _logger.LogError("{Error}: Error creating user: {UserName}",ex.InnerException?.Message ,register.UserName);
            return StatusCode(500, new {Status = "Error", message = "REGISTER :endpoint: Error creating user during registration", errorMessage = ex.InnerException?.Message});
        }
    }


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] Login login)
    {
        try
        {
            //use promise all to fetch user by email and username

            var userEmailExist = await _userManager.FindByEmailAsync(login.Email);
            var userNameExist = await _userManager.FindByNameAsync(login.UserName);

            if (userEmailExist == null && userNameExist == null)
            {
                return BadRequest(new {Status = "Error", message = "User does not exist"});
            }

            var user = userEmailExist ?? userNameExist;
            if (user == null)
            {
                return BadRequest(new { Status = "Error", message = "User not found" });
            }

            var PasswordSignInCheck = await _userManager.CheckPasswordAsync(user, login.Password);
            
            if(!PasswordSignInCheck)
            {
                return BadRequest(new {Status = "Error", message = "Invalid password"});
            }

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var token = GetToken(authClaims);
            var refreshToken = new JwtSecurityTokenHandler().WriteToken(token);

            //store token in session
            HttpContext.Session.SetString("JWTToken", refreshToken);

            return Ok(new
            {
                Status = "Success",
                message = "User logged in successfully",
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userId = user.Id,
                userName = user.UserName,
                userEmail = user.Email
            });
        }
        catch(Exception ex)
        {
            _logger.LogError("{Error}: Error logging in user: {UserName}", ex.InnerException?.Message, login.UserName);
            return StatusCode(500, new {Status = "Error", message = "LOGIN :endpoint: Error logging in user", errorMessage = ex.Message});
        }
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var jwtSecret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured");
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:TokenValidityInMinutes"])),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }


    [HttpGet]
    [Route("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        try
        {
            if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new {Status = "Error", message = "Invalid email confirmation link"});
            }

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                return BadRequest(new {Status = "Error", message = "User not found"});
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if(!result.Succeeded)
            {
            return BadRequest(new {Status = "Error", message = "Error confirming email"});
            }

            return Ok(new {Status = "Success", message = "Email confirmed successfully"});
        }
        catch(Exception ex)
        {
            _logger.LogError("{Error}: Error confirming email for user: {UserId}", ex.InnerException?.Message, userId);
            return StatusCode(500, new {Status = "Error", message = "CONFIRM EMAIL :endpoint: Error confirming email", errorMessage = ex.Message});
        }
    }
}    