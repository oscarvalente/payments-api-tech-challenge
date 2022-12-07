using Microsoft.AspNetCore.Identity;
using PaymentsAPI.DataAccess;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Services;
using PaymentsAPI.Utils;
using PaymentsAPI.Extensions;
using PaymentsGatewayApi.WebApi.Services;
using PaymentsAPI.Payments.Services;
using PaymentsAPI.Metrics;
using Microsoft.EntityFrameworkCore;
using PaymentsAPI.Middlewares;

public static class ProgramConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder, string connectionString, MySqlServerVersion serverVersion)
    {
        builder.Services.AddDbContext<PaymentsAPIDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<PaymentsAPIDbContext>();

        // business logic services
        builder.Services.AddScoped<ISignUp, SignUp>();
        builder.Services.AddScoped<ISignIn, SignIn>();
        builder.Services.AddScoped<IPayment, Payments>();
        builder.Services.AddScoped<IMerchant, Merchants>();
        builder.Services.AddSingleton<IToken, Token>();
        builder.Services.AddSingleton<IBankMatcher, BankMatcher>();
        builder.Services.AddSingleton<ICurrencyValidator, CurrencyValidator>();
        builder.Services.AddSingleton<IAPIResponseBuilder, APIReponseBuilder>();
        // data access
        builder.Services.AddScoped<IMerchantData, MerchantData>();
        builder.Services.AddScoped<IPaymentData, PaymentData>();
        // request-scoped merchant
        builder.Services.AddScoped<IRequesterMerchant, RequesterMerchant>();

        // register deps
        builder.Services.RegisterRequestHandlers();

        builder.Services.AddProblemDetailsMapper();
        builder.Services.AddControllers(options => options.Filters.Add<RequestProcessingTimeLoggerFilterAttribute>());

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSwaggerGen();
    }

    public static void Configure(this WebApplication app)
    {
        app.UseHttpLogging();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseSwagger();
            app.UseSwaggerUI();
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

        app.UseProblemDetailsMapper();
        // Use the UseWhen method to register the middleware only when the predicate is true
        app.UseWhen(ctx => ctx.Request.Path.Value.StartsWith("/api/pay"), appBuilder =>
        {
            appBuilder.UseMiddleware<AuthenticationValidator>();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}