using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using InterviewShowcase.Api.Contracts;
using InterviewShowcase.Api.Data;
using InterviewShowcase.Api.Models;
using InterviewShowcase.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = GetAllowedOrigins(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "InterviewShowcase.Api" }));

var authGroup = app.MapGroup("/api/auth");

authGroup.MapPost("/register", async (
    RegisterRequest request,
    AppDbContext dbContext,
    JwtTokenService tokenService) =>
{
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var normalizedEmail = request.Email.Trim().ToLowerInvariant();
    var existingUser = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail);
    if (existingUser)
    {
        return Results.Conflict(new { message = "Email is already registered." });
    }

    var (hash, salt) = PasswordService.CreateHash(request.Password);
    var user = new AppUser
    {
        Id = Guid.NewGuid(),
        Email = normalizedEmail,
        PasswordHash = hash,
        PasswordSalt = salt,
        CreatedOnUtc = DateTime.UtcNow
    };

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    var (token, expiresAtUtc) = tokenService.CreateToken(user);
    return Results.Ok(new AuthResponse(token, expiresAtUtc, user.Id, user.Email));
});

authGroup.MapPost("/login", async (
    LoginRequest request,
    AppDbContext dbContext,
    JwtTokenService tokenService) =>
{
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var normalizedEmail = request.Email.Trim().ToLowerInvariant();
    var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail);
    if (user is null || !PasswordService.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
    {
        return Results.Unauthorized();
    }

    var (token, expiresAtUtc) = tokenService.CreateToken(user);
    return Results.Ok(new AuthResponse(token, expiresAtUtc, user.Id, user.Email));
});

var apiGroup = app.MapGroup("/api").RequireAuthorization();

apiGroup.MapGet("/projects", async (ClaimsPrincipal user, AppDbContext dbContext) =>
{
    var userId = GetUserId(user);

    var projects = await dbContext.Projects
        .Where(p => p.OwnerId == userId)
        .OrderByDescending(p => p.CreatedOnUtc)
        .Select(p => new ProjectResponse(p.Id, p.Name, p.Description, p.Status, p.CreatedOnUtc))
        .ToListAsync();

    return Results.Ok(projects);
});

apiGroup.MapPost("/projects", async (ClaimsPrincipal user, CreateProjectRequest request, AppDbContext dbContext) =>
{
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var project = new ProjectEntity
    {
        Id = Guid.NewGuid(),
        Name = request.Name.Trim(),
        Description = request.Description.Trim(),
        Status = request.Status,
        CreatedOnUtc = DateTime.UtcNow,
        OwnerId = GetUserId(user)
    };

    dbContext.Projects.Add(project);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/projects/{project.Id}", new ProjectResponse(project.Id, project.Name, project.Description, project.Status, project.CreatedOnUtc));
});

apiGroup.MapPut("/projects/{id:guid}", async (Guid id, ClaimsPrincipal user, UpdateProjectRequest request, AppDbContext dbContext) =>
{
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var userId = GetUserId(user);
    var project = await dbContext.Projects.SingleOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);
    if (project is null)
    {
        return Results.NotFound();
    }

    project.Name = request.Name.Trim();
    project.Description = request.Description.Trim();
    project.Status = request.Status;

    await dbContext.SaveChangesAsync();
    return Results.Ok(new ProjectResponse(project.Id, project.Name, project.Description, project.Status, project.CreatedOnUtc));
});

apiGroup.MapDelete("/projects/{id:guid}", async (Guid id, ClaimsPrincipal user, AppDbContext dbContext) =>
{
    var userId = GetUserId(user);
    var project = await dbContext.Projects.SingleOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);
    if (project is null)
    {
        return Results.NotFound();
    }

    dbContext.Projects.Remove(project);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

apiGroup.MapGet("/tasks", async (ClaimsPrincipal user, AppDbContext dbContext) =>
{
    var userId = GetUserId(user);

    var tasks = await dbContext.Tasks
        .Include(t => t.Project)
        .Where(t => t.Project != null && t.Project.OwnerId == userId)
        .OrderBy(t => t.DueDate)
        .Select(t => new TaskResponse(t.Id, t.Title, t.Priority, t.State, t.DueDate, t.ProjectId, t.CreatedOnUtc))
        .ToListAsync();

    return Results.Ok(tasks);
});

apiGroup.MapPost("/tasks", async (ClaimsPrincipal user, CreateTaskRequest request, AppDbContext dbContext) =>
{
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var userId = GetUserId(user);
    var projectExists = await dbContext.Projects.AnyAsync(p => p.Id == request.ProjectId && p.OwnerId == userId);
    if (!projectExists)
    {
        return Results.BadRequest(new { message = "Project does not exist for this user." });
    }

    var task = new TaskItemEntity
    {
        Id = Guid.NewGuid(),
        Title = request.Title.Trim(),
        Priority = request.Priority,
        State = request.State,
        DueDate = request.DueDate,
        ProjectId = request.ProjectId,
        CreatedOnUtc = DateTime.UtcNow
    };

    dbContext.Tasks.Add(task);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/tasks/{task.Id}", new TaskResponse(task.Id, task.Title, task.Priority, task.State, task.DueDate, task.ProjectId, task.CreatedOnUtc));
});

apiGroup.MapPut("/tasks/{id:guid}", async (Guid id, ClaimsPrincipal user, UpdateTaskRequest request, AppDbContext dbContext) =>
{
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var userId = GetUserId(user);
    var task = await dbContext.Tasks
        .Include(t => t.Project)
        .SingleOrDefaultAsync(t => t.Id == id && t.Project != null && t.Project.OwnerId == userId);
    if (task is null)
    {
        return Results.NotFound();
    }

    var projectExists = await dbContext.Projects.AnyAsync(p => p.Id == request.ProjectId && p.OwnerId == userId);
    if (!projectExists)
    {
        return Results.BadRequest(new { message = "Project does not exist for this user." });
    }

    task.Title = request.Title.Trim();
    task.Priority = request.Priority;
    task.State = request.State;
    task.DueDate = request.DueDate;
    task.ProjectId = request.ProjectId;

    await dbContext.SaveChangesAsync();
    return Results.Ok(new TaskResponse(task.Id, task.Title, task.Priority, task.State, task.DueDate, task.ProjectId, task.CreatedOnUtc));
});

apiGroup.MapDelete("/tasks/{id:guid}", async (Guid id, ClaimsPrincipal user, AppDbContext dbContext) =>
{
    var userId = GetUserId(user);
    var task = await dbContext.Tasks
        .Include(t => t.Project)
        .SingleOrDefaultAsync(t => t.Id == id && t.Project != null && t.Project.OwnerId == userId);
    if (task is null)
    {
        return Results.NotFound();
    }

    dbContext.Tasks.Remove(task);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

apiGroup.MapGet("/dashboard/summary", async (ClaimsPrincipal user, AppDbContext dbContext) =>
{
    var userId = GetUserId(user);

    var projectsCount = await dbContext.Projects.CountAsync(p => p.OwnerId == userId);
    var userTasks = dbContext.Tasks.Where(t => t.Project != null && t.Project.OwnerId == userId);

    var tasksCount = await userTasks.CountAsync();
    var completedTasks = await userTasks.CountAsync(t => t.State == TaskState.Done);
    var inProgressTasks = await userTasks.CountAsync(t => t.State == TaskState.InProgress);
    var upcomingTasks = await userTasks.CountAsync(t => t.DueDate >= DateOnly.FromDateTime(DateTime.UtcNow) && t.State != TaskState.Done);

    var summary = new DashboardSummary(projectsCount, tasksCount, completedTasks, inProgressTasks, upcomingTasks);
    return Results.Ok(summary);
});

app.Run();

static Guid GetUserId(ClaimsPrincipal user)
{
    var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(value, out var userId))
    {
        throw new UnauthorizedAccessException("Invalid JWT subject claim.");
    }

    return userId;
}

static Dictionary<string, string[]> Validate(object model)
{
    var context = new ValidationContext(model);
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(model, context, results, true);

    return results
        .Where(r => !string.IsNullOrWhiteSpace(r.ErrorMessage))
        .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty), (result, memberName) => new { memberName, error = result.ErrorMessage! })
        .GroupBy(item => item.memberName)
        .ToDictionary(g => g.Key, g => g.Select(x => x.error).ToArray());
}

static string[] GetAllowedOrigins(IConfiguration configuration)
{
    var configuredOrigins = configuration["CORS_ALLOWED_ORIGINS"];
    if (!string.IsNullOrWhiteSpace(configuredOrigins))
    {
        return configuredOrigins
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    return ["http://localhost:5173", "http://localhost:3000"];
}
