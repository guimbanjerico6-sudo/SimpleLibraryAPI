using SimpleLibraryAPI.DAL;
using SimpleLibraryAPI.Services;

namespace SimpleLibraryAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- ADD THIS LINE HERE ---
            // This tells the API how to create your Service
            builder.Services.AddSingleton<SimpleLibraryAPI.Services.ProductService>();
            builder.Services.AddSingleton<SimpleLibraryAPI.Services.BookService>();
            builder.Services.AddSingleton<SimpleLibraryAPI.DAL.BookRepository>();
            // --------------------------


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            //Mikee - inaadd ko to para mag connect
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    // Palitan ang port dito para tumugma sa UI mo (7244)
                    policy.WithOrigins("https://localhost:7244", "http://localhost:7244")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddSingleton<BookRepository>();
            builder.Services.AddScoped<BookService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            if (app.Environment.IsDevelopment())
            {
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

            //ako din nag add ng allowFrontend
            app.UseCors("AllowFrontend");

            app.MapControllers();
            app.UseMiddleware<ExceptionMiddleware>();

            app.Run();
        }
    }
}