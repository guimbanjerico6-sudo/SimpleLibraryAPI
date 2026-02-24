namespace SimpleLibraryAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- DEPENDENCY INJECTION (The Matchmakers) ---

            // (Optional: Leave this if you are still using the ProductService from earlier!)
            builder.Services.AddSingleton<SimpleLibraryAPI.Services.ProductService>();

            // 1. The DAL Matchmaker (Contract -> Implementation)
            builder.Services.AddSingleton<SimpleLibraryAPI.DAL.IBookRepository, SimpleLibraryAPI.DAL.BookRepository>();

            // 2. The Service Matchmaker (Contract -> Implementation)
            builder.Services.AddSingleton<SimpleLibraryAPI.Services.IBookService, SimpleLibraryAPI.Services.BookService>();

            // ----------------------------------------------

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            // --- MIDDLEWARE PIPELINE ---

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                // In development, it's okay to see the full error for debugging
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // In PRODUCTION, we use the Safety Net
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500; // Professional "Internal Server Error"
                        context.Response.ContentType = "application/json";

                        // We send back a clean, polite message
                        var errorResponse = new { message = "An unexpected error occurred. Our engineers are on it!" };
                        await context.Response.WriteAsJsonAsync(errorResponse);
                    });
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.UseMiddleware<ExceptionMiddleware>();

            app.Run();
        }
    }
}