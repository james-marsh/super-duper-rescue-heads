using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Api.Authorization;
using SuperDuperRescueHeads.Api.Endpoints;
using SuperDuperRescueHeads.Api.Hubs;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Domain.Notifications;
using SuperDuperRescueHeads.Domain.Search;
using SuperDuperRescueHeads.Domain.Sharing;
using SuperDuperRescueHeads.Infrastructure.Data;
using SuperDuperRescueHeads.Infrastructure.Data.Repositories;
using SuperDuperRescueHeads.Infrastructure.BackgroundJobs;
using SuperDuperRescueHeads.Infrastructure.Repositories;
using SuperDuperRescueHeads.Infrastructure.Search;
using SuperDuperRescueHeads.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache(); // Feature 007: For caching group memberships
builder.Services.AddSignalR(); // Feature 008: Real-time notifications

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=SuperDuperRescueHeads;Trusted_Connection=true;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ICollectionShareRepository, CollectionShareRepository>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IConflictEventRepository, ConflictEventRepository>();

// Search (Feature 004)
builder.Services.AddScoped<ISearchRepository, SearchRepository>();
builder.Services.AddScoped<ISearchService, SearchService>();

// Sharing (Feature 006)
builder.Services.AddScoped<IEmailService, EmailService>();

// Group Sharing (Feature 007)
builder.Services.AddScoped<IGroupSyncService, GroupSyncService>();

// Notifications (Feature 008)
builder.Services.AddScoped<INotificationService, NotificationService>();

// Concurrent Editing (Feature 009)
builder.Services.AddScoped<IConflictResolutionService, ConflictResolutionService>();

// Hangfire for background jobs (Feature 003)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

// Background jobs
builder.Services.AddScoped<PurgeDeletedItemsJob>();
builder.Services.AddScoped<SyncGroupMembershipsJob>();

// Authorization (Feature 006)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, CollectionPermissionHandler>();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHangfireDashboard("/hangfire"); // Hangfire dashboard
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map API endpoints
app.MapItemsEndpoints();
app.MapDeletedItemsEndpoints();
app.MapSearchEndpoints(); // Feature 004
app.MapCollectionSharingEndpoints(); // Feature 006
app.MapGroupSharingEndpoints(); // Feature 007
app.MapGroupMembershipWebhooks(); // Feature 007 - Webhooks for real-time sync
app.MapNotificationEndpoints(); // Feature 008

// Map SignalR hubs (Feature 008)
app.MapHub<NotificationHub>("/hubs/notifications");

// Schedule recurring jobs (Feature 003 - User Story 5)
RecurringJob.AddOrUpdate<PurgeDeletedItemsJob>(
    "purge-deleted-items",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily(2)); // Run daily at 2 AM

// Schedule group membership sync job (Feature 007 - User Story 2)
// Runs every 30 seconds to meet SC-044 and SC-045 requirements
RecurringJob.AddOrUpdate<SyncGroupMembershipsJob>(
    "sync-group-memberships",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/30 * * * * *"); // Every 30 seconds

app.Run();

