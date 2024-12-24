using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityServerAspNetIdentity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure the DbContext for IdentityServer
    builder.Services.AddDbContext<ConfigurationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDbContext<PersistedGrantDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //     options.UseSqlite(
    //         builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddRazorPages();

    builder.Services.AddIdentityServer()
        // .AddInMemoryClients([
        //     new Client
        //     {
        //         ClientId = "client",
        //         AllowedGrantTypes = GrantTypes.Implicit,
        //         RedirectUris = { "https://localhost:5002/signin-oidc" },
        //         PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
        //         FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
        //         AllowedScopes = { "openid", "profile", "email", "phone" }
        //     }
        // ])
        // .AddInMemoryIdentityResources([
        //     new IdentityResources.OpenId(),
        //     new IdentityResources.Profile(),
        //     new IdentityResources.Email(),
        //     new IdentityResources.Phone(),
        // ])
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = db =>
                db.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
        })
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = db =>
                db.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
        })
        .AddAspNetIdentity<IdentityUser>();

    builder.Services.AddLogging(options =>
    {
        options.AddFilter("Duende", LogLevel.Debug);
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();


    // Apply migrations to both Configuration and PersistedGrantDbContexts
    using (var scope = app.Services.CreateScope())
    {
        var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        configContext.Database.Migrate(); // Apply migrations for IdentityServer configuration

        var persistedGrantContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        persistedGrantContext.Database.Migrate(); // Apply migrations for operational data
    }

    app.UseRouting();

    app.UseIdentityServer();
    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException")
{
    Console.WriteLine(ex.Message);
}
