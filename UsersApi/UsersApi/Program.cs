using Microsoft.EntityFrameworkCore;
using UsersApi.Models;

var builder = WebApplication.CreateBuilder(args);
string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddControllers();
builder.Services.AddDbContext<UserContext>(opt => opt.UseSqlServer(connection));
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
