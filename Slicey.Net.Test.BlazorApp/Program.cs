using Slicey.Net.Test.BlazorApp.Components;
using Slicey.Net.Test.BlazorApp.State;

namespace Slicey.Net.Test.BlazorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.RegisterRootStore<RootStore, AppState>(new AppState());
            builder.RegisterSliceStore<EchoStateStore, AppState, EchoState>();
            builder.RegisterSliceStore<CounterStateStore, AppState, CounterState>();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
