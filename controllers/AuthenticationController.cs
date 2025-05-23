using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NanoidDotNet;
using SocialMediaAPI.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Numbers = "0123456789";
    private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    private const string GuidSize = UppercaseLetters + LowercaseLetters + Numbers;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IEmailService _emailService;

    public AuthenticationController(UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger, IEmailService emailService)
    {
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
    }

    [HttpPost]
    [Route("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
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
                Id = Nanoid.Generate(GuidSize, 8),
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

            _=Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(user.Email, "Confirm your email => SocialMediaApi", $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>click here</a>");
                }
                catch(Exception ex)
                {
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    _logger.LogError("{Error}: Error sending email to user: {UserName}", errorMessage, register.UserName);
                }
            });

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
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
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT:Secret is not configured");

        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER environment variable is not configured");

        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE environment variable is not configured");
        
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            expires: DateTime.Now.AddHours(Environment.GetEnvironmentVariable("JWT_EXPIRATION") != null ? Convert.ToDouble(Environment.GetEnvironmentVariable("JWT_EXPIRATION")) : 1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }


    [HttpGet]
    [Route("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmail(string userId, string token = "null")
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                return BadRequest(new {Status = "Error", message = "User not found"});
            }

            if(token == "null")
            {
                user = await _userManager.FindByIdAsync(userId);

                token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                if(token == null)
                {
                    return BadRequest(new {Status = "Error", message = "Error generating email confirmation token"});
                }

                var confirmationLink = Url.Action("ConfirmEmail", "Authentication", new { userId = user.Id, Token = token }, Request.Scheme);
                if(confirmationLink == null)
                {
                    return BadRequest(new {Status = "Error", message = "Error generating email confirmation link"});
                }

                _=Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(user.Email, "Confirm your email => SocialMediaApi", $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>click here</a>");
                    }
                    catch(Exception ex)
                    {
                        var errorMessage = ex.InnerException?.Message ?? ex.Message;
                        _logger.LogError("{Error}: Error sending email to user: {UserName}", errorMessage, user.UserName);
                    }
                });
            }

            if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new {Status = "Error", message = "Invalid email confirmation link"});
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

    [HttpPost]
    [Route("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return BadRequest(new { Status = "Error", Message = "User not found" });

            // Clear session
            await Task.Run(() => HttpContext.Session.Clear());

            _logger.LogInformation("User {UserId} logged out successfully", userId);

            return Ok(new { Status = "Success", Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError("{Error}: Error logging out user", ex.InnerException?.Message);
            return StatusCode(500, new { Status = "Error", Message = "Error during logout", ErrorMessage = ex.Message });
        }
    }
}