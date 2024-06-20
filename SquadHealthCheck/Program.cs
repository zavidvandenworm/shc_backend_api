using ApplicationEF.Services;
using dotenv.net;
using Infrastructure;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AiService>();
builder.Services.AddInfrastructure();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowDevApi",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Use the configured CORS policy
app.UseCors("allowDevApi");

app.MapControllers();

app.Run();