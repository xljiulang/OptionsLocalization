using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OptionsLocalization;
using WebApp.Controllers;

namespace WebApp
{
    sealed class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder
                .AddOptionsLocalizer("en", ["en", "fr", "zh"])
                .Configure<AppOptions>()
                .Configure<HomeOptions>();

            builder.Services.AddControllers();
            var app = builder.Build();

            // 中间件设置当前线程的语言区域
            app.Use(next => context =>
            {
                context.Request.Query.TryGetValue("culture", out var culture);
                OptionsLocalizer.SetCurrentThreadCulture(culture, "en");
                return next(context);
            });

            app.MapControllers();
            app.Run();
        }
    }
}
