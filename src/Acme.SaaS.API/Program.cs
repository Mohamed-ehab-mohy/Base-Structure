using Acme.SaaS.API.Extensions;
using Acme.SaaS.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiLayer(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddTenantDbContext(builder.Configuration);

var app = builder.Build();
app.UseApiPipeline();
app.Run();
