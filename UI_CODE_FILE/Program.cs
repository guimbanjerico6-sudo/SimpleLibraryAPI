
namespace UI_CODE_FILE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Ito ang kailangan para basahin ng program ang index.html mo sa wwwroot
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run();
        }
    }
}

