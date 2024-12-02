// DepotWebAPI/Program.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DepotWebAPI.Data; // Namespace voor ApplicationDbContext
using DepotWebAPI.Models; // Namespace voor RegisterRequest
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RegisterRequest = DepotWebAPI.Models.RegisterRequest;
using LoginRequest = DepotWebAPI.Models.LoginRequest;

var builder = WebApplication.CreateBuilder(args);

// 1. Voeg CORS-services toe aan de container
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()  // Staat alle origins toe
              .AllowAnyHeader()  // Staat elke header toe
              .AllowAnyMethod(); // Staat elke HTTP-methode toe
    });
});

// 2. Voeg andere services toe aan de container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configureer Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false; // Pas wachtwoordvereisten aan als nodig
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Voeg Newtonsoft.Json toe
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// JWT-configuratie
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured."))),
    };
});

// Voeg de autorisatiebeleid toe vóór Build()
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireClaim("IsAdmin", "True")); // Controleer de IsAdmin claim
});

// Swagger-configuratie
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Login API", Version = "v1" });
});

var app = builder.Build();

// Zorg dat de database gemigreerd is bij het starten
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // Voer migraties uit
}

// Configureer de HTTP-aanvraagpipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Gebruik het CORS-beleid vóór authentication en authorization
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

// Registratie endpoint
app.MapPost("/register", async (UserManager<ApplicationUser> userManager, RegisterRequest request) =>
{
    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        FullName = request.Name, // Zorg ervoor dat dit overeenkomt met je model
        IsAdmin = request.IsAdmin // Stel IsAdmin in op basis van de request
    };

    var result = await userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
    {
        return Results.Ok("Account successfully created.");
    }

    return Results.BadRequest(result.Errors);
});

// Inloggen endpoint
app.MapPost("/login", async (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, [FromBody] LoginRequest request) =>
{
    var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
    if (!result.Succeeded)
    {
        return Results.BadRequest("Login mislukt. Controleer uw gebruikersnaam en wachtwoord.");
    }

    var user = await userManager.FindByEmailAsync(request.Email);
    if (user == null)
    {
        return Results.BadRequest("User not found.");
    }

    var token = GenerateJwtToken(user);
    return Results.Ok(new
    {
        tokenType = "Bearer",
        accessToken = token,
        expiresIn = 3600 // Verlooptijd in seconden
    });
});

// Voorbeeld van een ander endpoint
app.MapGet("/users", async (UserManager<ApplicationUser> userManager) =>
{
    var users = await userManager.Users.ToListAsync();
    return Results.Ok(users);
})
.RequireAuthorization("RequireAdminRole"); // Vereist admin rol

// JWT-token genereren
string GenerateJwtToken(ApplicationUser user)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty), // Default waarde voor UserName
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("IsAdmin", user.IsAdmin.ToString()) // Claim toevoegen voor admin status
    };

    var key = builder.Configuration["Jwt:Key"];
    if (key == null)
    {
        throw new InvalidOperationException("JWT key is not configured.");
    }

    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: null,
        audience: null,
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
};

app.MapGet("/me", async (UserManager<ApplicationUser> userManager, HttpContext httpContext) =>
{
    // Haal het e-mailadres uit de claims (de 'sub' claim bevat het e-mailadres)
    var emailClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    if (string.IsNullOrEmpty(emailClaim))
    {
        return Results.BadRequest("Geen geldige email gevonden in het token.");
    }

    // Zoek de gebruiker op basis van het e-mailadres
    var user = await userManager.FindByEmailAsync(emailClaim);

    if (user == null)
    {
        return Results.NotFound("Gebruiker niet gevonden.");
    }

    // Geef de id, naam en email terug als JSON
    return Results.Ok(new
    {
        UserId = user.Id,     // Voeg de UserId toe aan de response
        Name = user.FullName, // Zorg ervoor dat FullName aanwezig is in het ApplicationUser-model
        Email = user.Email
    });
})
.RequireAuthorization();

// POST endpoint voor Analytics
app.MapPost("/analytics", async (ApplicationDbContext dbContext, AnalyticsRequest request) =>
{
    // Zoek naar de eerste Analytics-entry in de database
    var analytics = await dbContext.Analytics.FirstOrDefaultAsync();

    // Als er geen entry is, maak er een aan
    if (analytics == null)
    {
        analytics = new Analytics();
        dbContext.Analytics.Add(analytics);
    }

    // Werk de juiste property bij op basis van de button naam en amount
    switch (request.Button.ToLower())
    {
        case "home":
            analytics.Home += request.Amount;
            break;
        case "store":
            analytics.Store += request.Amount;
            break;
        case "implementation":
            analytics.Implementation += request.Amount;
            break;
        case "aboutus":
            analytics.AboutUs += request.Amount;
            break;
        case "contact":
            analytics.Contact += request.Amount;
            break;
        default:
            return Results.BadRequest("Onbekende knopnaam.");
    }

    await dbContext.SaveChangesAsync();

    return Results.Ok(analytics); // Retourneer de bijgewerkte analytics data
});


// GET endpoint voor Analytics (nu met vereiste admin rol)
app.MapGet("/analyticsdb", async (ApplicationDbContext dbContext) =>
{
    var analyticsData = await dbContext.Analytics.ToListAsync();
    return Results.Ok(analyticsData);
})
.RequireAuthorization("RequireAdminRole"); // Vereist admin rol


// Laat de applicatie draaien
app.Run();
