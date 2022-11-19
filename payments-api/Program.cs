using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaymentsAPI.DataAccess;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("Default");
var serverVersion = new MySqlServerVersion(new Version(6, 0, 2));
builder.Services.AddDbContext<PaymentsAPIDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<PaymentsAPIDbContext>();

// business logic services
builder.Services.AddScoped<ISignUp, SignUp>();
builder.Services.AddScoped<ISignIn, SignIn>();
builder.Services.AddScoped<IPayment, Payments>();
builder.Services.AddScoped<IMerchant, Merchant>();
builder.Services.AddSingleton<IToken, Token>();
// data access
builder.Services.AddScoped<IMerchantData, MerchantData>();
builder.Services.AddScoped<IPaymentData, PaymentData>();

builder.Services.AddControllers();


var app = builder.Build();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// for tests
public partial class Program { }