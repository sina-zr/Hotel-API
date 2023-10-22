using HotelAPI.StartupConfig;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencyInjections();
builder.AddEFCore();
builder.AddAuthentication();
builder.AddVersioning();
builder.AddStandardServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();