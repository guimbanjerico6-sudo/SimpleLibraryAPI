namespace SimpleLibraryAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- DEPENDENCY INJECTION (The Matchmakers) ---
            builder.Services.AddSingleton<SimpleLibraryAPI.Services.ProductService>();
            builder.Services.AddSingleton<SimpleLibraryAPI.DAL.IBookRepository, SimpleLibraryAPI.DAL.BookRepository>();
            builder.Services.AddSingleton<SimpleLibraryAPI.Services.IBookService, SimpleLibraryAPI.Services.BookService>();

            // --- THE VIP LIST (CORS) ---
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173") // Your React Frontend
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

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
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        var errorResponse = new { message = "An unexpected error occurred. Our engineers are on it!" };
                        await context.Response.WriteAsJsonAsync(errorResponse);
                    });
                });
            }

            // --- ACTIVATE THE VIP LIST ---
            app.UseCors("AllowReactApp"); // Must be before Authorization!

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Your custom error handler
            app.UseMiddleware<ExceptionMiddleware>();

            app.Run();
        }
    }
}