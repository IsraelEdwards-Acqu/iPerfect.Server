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

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Root endpoint
app.MapGet("/", () => "iPerfect Server is running...");

app.Run();
