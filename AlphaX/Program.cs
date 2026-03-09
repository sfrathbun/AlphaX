using Google.Cloud.Firestore;
using AlphaX.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services
    .AddControllers()
    .AddNewtonsoftJson();  // ← Add this line
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Firebase configuration
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
    builder.Configuration["Firebase:CredentialsPath"]);

var firestoreDb = FirestoreDb.Create(firebaseProjectId);
builder.Services.AddSingleton(firestoreDb);

// Register custom services
builder.Services.AddScoped<FirebaseService>();
builder.Services.AddScoped<ComplianceEngine>();

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "API Running")
    .WithName("Root");

app.Run("https://localhost:7003");