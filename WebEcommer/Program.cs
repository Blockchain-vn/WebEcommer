using Microsoft.EntityFrameworkCore;
using WebEcommer.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register DbContext for EcommerDemo
builder.Services.AddDbContext<EcommerDemoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectString")));

// Configure MailSettings
builder.Services.Configure<WebEcommer.Helpers.MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<WebEcommer.Services.IEmailSender, WebEcommer.Services.EmailSender>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authorization
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
