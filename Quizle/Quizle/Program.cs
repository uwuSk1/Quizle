using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quizle.Core.Questions.Contracts;
using Quizle.Core.Questions.Services;
using Quizle.Data;
using Quizle.DB;
using Quizle.DB.Models;
using Quizle.Web.MapperProfiles;
using Quizle.Web.Infra.QuartzJobs;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QuizleDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<QuizleDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/Login";
});
builder.Services.AddScoped<ITriviaDataService, TriviaDataService>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<QuizMapperProfile>();
});
builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));


builder.Services.AddQuartz(q =>
{
    q.SchedulerId = "Scheduler-Core";

    // we take this from appsettings.json, just show it's possible
    // q.SchedulerName = "Quartz ASP.NET Core Sample Scheduler";

    // as of 3.3.2 this also injects scoped services (like EF DbContext) without problems
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });

    q.ScheduleJob<TriviaJob>(trigger => trigger
            .WithIdentity("Combined Configuration Trigger")
            .StartNow()
            .WithDailyTimeIntervalSchedule(x => x.StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0,0)).OnEveryDay())            
        );
});
builder.Services.AddQuartzHostedService(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});






var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//app.MapRazorPages();

app.Run();
