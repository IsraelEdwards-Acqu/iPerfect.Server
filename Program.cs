using iPerfect.Services;
using iPerfect.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Register core services
builder.Services.AddScoped<ImageAnalysisService>();
builder.Services.AddScoped<MetadataAnalyzer>();
builder.Services.AddScoped<ElaAnalyzer>();
builder.Services.AddScoped<AiService>();          // AI detection (remote + local fallback)
builder.Services.AddScoped<PrnuService>();        // PRNU fingerprinting
builder.Services.AddScoped<FileFormatService>();

// Add HttpClient factory for AiService remote calls
builder.Services.AddHttpClient();

// ✅ Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNetlify", policy =>
    {
        policy.WithOrigins("https://iperfect.netlify.app") // your Netlify domain
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

// ✅ Enable CORS before mapping controllers
app.UseCors("AllowNetlify");

// Map controllers
app.MapControllers();

// Root endpoint
app.MapGet("/", () => "iPerfect Server is running...");

app.Run();
