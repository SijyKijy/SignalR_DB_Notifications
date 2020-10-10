using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Server.Hubs;
using Server.Services;

namespace SignalR_Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<DbRepository>()
                .AddSingleton<IUserIdProvider, CustomUserIdProvider>()
                .AddSingleton<ContextCollection<string>>();
            services.AddSignalR();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.ApplicationServices.GetRequiredService<DbRepository>().Registration();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TestHub>("/hub");
                endpoints.MapControllers();
            });
        }
    }
}
