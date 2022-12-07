
using Microsoft.EntityFrameworkCore;
using PaymentsAPI.Middlewares;
using PaymentsAPI.DataAccess;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Services;
using PaymentsAPI.Utils;
using PaymentsAPI.Extensions;
using PaymentsGatewayApi.WebApi.Services;
using PaymentsAPI.Payments.Services;
using PaymentsAPI.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("Default");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));
builder.ConfigureServices(connectionString, serverVersion);

var app = builder.Build();

app.Configure();

app.Run();

// for tests
public partial class Program { }