using Google.Cloud.Firestore;
using Google.Cloud.PubSub.V1;
using Google.Cloud.Storage.V1;
using PLC.Application.Handlers;
using PLC.Infrastructure.Configuration;
using PLC.Infrastructure.Interfaces;
using PLC.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<GoogleCloudOptions>(
    builder.Configuration.GetSection(GoogleCloudOptions.SectionName));

// Google Cloud Services
var googleCloudOptions = builder.Configuration
    .GetSection(GoogleCloudOptions.SectionName)
    .Get<GoogleCloudOptions>();

if (googleCloudOptions == null)
    throw new InvalidOperationException("GoogleCloud configuration is missing");

// Storage Client
builder.Services.AddSingleton(StorageClient.Create());

// Firestore Client
builder.Services.AddSingleton(FirestoreDb.Create(googleCloudOptions.ProjectId));

// Pub/Sub Publisher Client
builder.Services.AddSingleton(sp =>
{
    var topicName = new TopicName(googleCloudOptions.ProjectId, googleCloudOptions.PubSubTopic);
    return PublisherClient.CreateAsync(topicName).GetAwaiter().GetResult();
});

// Infrastructure Services
builder.Services.AddScoped<IDocumentStorageService, GoogleCloudStorageService>();
builder.Services.AddScoped<IDocumentRepository, FirestoreDocumentRepository>();
builder.Services.AddScoped<IEventPublisher, GooglePubSubPublisher>();

// Application Handlers
builder.Services.AddScoped<UploadDocumentHandler>();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
