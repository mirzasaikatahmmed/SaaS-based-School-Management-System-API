using Serilog;
using SchoolManagement.API.Extensions;
using SchoolManagement.API.Filters;
using SchoolManagement.API.Middleware;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    });

    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Management API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "School Management API";
            c.HeadContent = """
                <style>
                  .endpoint-counter-banner {
                    background: #1b1b1b;
                    color: #fff;
                    font: 600 13px/1.4 system-ui, sans-serif;
                    padding: 10px 16px;
                    border-bottom: 1px solid #333;
                  }
                  .endpoint-counter-banner span { opacity: .85; font-weight: 500; }
                </style>
                <script>
                  window.addEventListener('load', async () => {
                    try {
                      const res = await fetch('/swagger/v1/swagger.json');
                      const doc = await res.json();
                      let total = 0;
                      const methods = {};
                      for (const path of Object.values(doc.paths || {})) {
                        for (const method of Object.keys(path)) {
                          if (['get','post','put','patch','delete','head','options','trace'].includes(method)) {
                            total++;
                            methods[method.toUpperCase()] = (methods[method.toUpperCase()] || 0) + 1;
                          }
                        }
                      }
                      const order = ['GET','POST','PATCH','DELETE','PUT'];
                      const parts = order.filter(m => methods[m]).map(m => m + ': ' + methods[m]);
                      const banner = document.createElement('div');
                      banner.className = 'endpoint-counter-banner';
                      banner.innerHTML = 'Total endpoints: <strong>' + total + '</strong> &nbsp; <span>' + parts.join(' · ') + '</span>';
                      const topbar = document.querySelector('.topbar') || document.body;
                      topbar.parentNode.insertBefore(banner, topbar.nextSibling);
                      document.title = 'School Management API — ' + total + ' endpoints';
                    } catch (e) { console.warn('Endpoint counter failed', e); }
                  });
                </script>
                """;
        });
    }

    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseMiddleware<TenantResolutionMiddleware>();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    await app.InitializeApplicationAsync();

    Log.Information("School Management API starting...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
