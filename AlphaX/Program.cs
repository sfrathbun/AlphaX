using Google.Cloud.Firestore;
using Google.Apis.Auth;
using Google.Api.Gax;
using AlphaX.Services;
using AlphaX.Middleware;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Firebase initialization
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
var credentialsPath = builder.Configuration["Firebase:CredentialsPath"];

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

// Initialize Firebase Admin SDK
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(credentialsPath),
    ProjectId = firebaseProjectId
});

// Firestore setup
var firestoreDb = FirestoreDb.Create(firebaseProjectId);
builder.Services.AddSingleton(firestoreDb);

builder.Services.AddScoped<ScanDataService>();

// Add services
builder.Services
    .AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Add Firebase authentication middleware BEFORE authorization
app.UseMiddleware<FirebaseAuthenticationMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "API Running")
    .WithName("Root");

app.Run("https://localhost:7003");


//using Google.Cloud.Firestore;
//using AlphaX.Services;
//using FirebaseAdmin;
//using Google.Apis.Auth.OAuth2;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddScoped<ScanDataService>();

//// Add services
//builder.Services
//    .AddControllers()
//    .AddNewtonsoftJson();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Firebase configuration
//var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
//Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
//    builder.Configuration["Firebase:CredentialsPath"]);

//var firestoreDb = FirestoreDb.Create(firebaseProjectId);
//builder.Services.AddSingleton(firestoreDb);

//// Register custom services
//builder.Services.AddScoped<FirebaseService>();
//builder.Services.AddScoped<ComplianceEngine>();

//// CORS for React frontend
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReact", policy =>
//    {
//        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
//            .AllowAnyMethod()
//            .AllowAnyHeader();
//    });
//});

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//FirebaseApp.Create(new AppOptions()
//{
//    Credential = GoogleCredential.FromFile("path/to/serviceAccountKey.json"),
//});

//app.UseHttpsRedirection();
//app.UseCors("AllowReact");
//app.UseAuthorization();
//app.MapControllers();

//app.MapGet("/", () => "API Running")
//    .WithName("Root");

//app.Run("https://localhost:7003");