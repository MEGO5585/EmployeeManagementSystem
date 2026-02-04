using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repostories.Contracts;
using ServerLibrary.Repostories.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("constr") ??
    throw new InvalidOperationException("sorry, your connection is not found")));
builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSection"));
builder.Services.AddScoped<IUserAccount, UserAccountRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ADD THESE CRITICAL MIDDLEWARE COMPONENTS:
app.UseHttpsRedirection();

// 1. Enable routing
app.UseRouting();

// 2. Optional: Add CORS if needed (especially for development)
app.UseCors(policy => 
    policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
          .AllowAnyHeader()
          .AllowAnyMethod());

// 3. Map controllers to endpoints - THIS IS WHAT YOU'RE MISSING!
app.UseEndpoints(endpoints =>
{
    _= endpoints.MapControllers();
});

// Alternative simpler approach (just add this line instead of UseEndpoints):
// app.MapControllers();

app.Run();