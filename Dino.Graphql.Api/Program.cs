using Dino.Graphql.Api;
using Dino.Graphql.Api.DbContexts;
using Dino.Graphql.Api.Mutations;
using Dino.Graphql.Api.Mutations.DbSelectors;
using Dino.Graphql.Api.Mutations.ModelViews;
using Dino.GraphqlLib.Extensions.GroupFiltes;
using Dino.GraphqlLib.Extensions.SortDefault;
using Dino.GraphqlLib.Infrastructures;
using Dino.GraphqlLib.Utilities;
using EntityGraphQL.AspNet;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// add graphql

builder.Services.AddDbContext<Graph1DbContext>(opt => opt.UseInMemoryDatabase("Demo1"));
builder.Services.AddDbContext<Graph2DbContext>(opt => opt.UseInMemoryDatabase("Demo2"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.Authority = "https://dev-vfwqs18vq3lg083q.us.auth0.com";
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
    };
    o.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
    };
    o.Events = new JwtBearerEvents
    {
        OnForbidden = async (context) =>
        {

        },
        OnTokenValidated = async (context) =>
        {

        },
        OnAuthenticationFailed = async (context) =>
        {

        },
        OnMessageReceived = async (context) =>
        {

        }
    };
});
builder.Services.AddAuthorization();

builder.Services.AddGraphql<ComplexGraphqlSchema>(builder =>
{
    builder
    //.ExtractSort<StudentDep>(x => new { id = x.Id })
    .AddFilterExpression<DbContext>() // This line will filter all fields that are DbSet in the 2 DbContexts in ComplexSchema and add extentions to it:
                                      //  - UseFilter.
                                      //  - UseOffsetPaging.
                                      //  - UseExpressionFilter (horizontal data fragmentation).
    .AddSiteRoleTransformation(context =>
    {
        var role = SiteHelper.GetRoleSite("ADMIN"); // admin-graph-site
        context.User.IsInRole(SiteHelper.GetRoleSite("ADMIN")); // True
        return context.Request.Path.StartsWithSegments("/admin") ? new[] { "ADMIN" } : null;
    })
    //.AddWhereClause<Student>(p => x => x.Name.Contains("hello1"))
    //.AddWhereClause<Teacher>(p => x => x.Name.Contains("hello3"))
    //.AddAuthorizeWhereClause<Subject>((opt) =>
    //{
    //    opt.AddRoles(x => x.RequiresAllRoles("Admin", SiteHelper.GetRoleSite("ADMINSITE")), p => x => x.Name == "hello2");
    //    opt.AddRoles(new string[] { "User", SiteHelper.GetRoleSite("ADMINSITE") }, p => x => x.Name == "hello1");
    //    opt.AddRoles(new string[] { "User" }, p => x => true);
    //})

    ;

    builder.FieldBuilder = b =>
    {
        b
        .AddMutationBuilder(typeof(DbContextService<,,>))
        .AddMutation<DbContextSelector1, Subject, SubjectModels.Create, SubjectModels.Update, SubjectModels.Key>()
        .AddMutation<DbContextSelector1, Student, StudentModels.Create, StudentModels.Update, StudentModels.Key>()
        ;

        b.ExtendField<Subject>("studentTest")
       .ResolveWithService<ComplexGraphqlSchema>((m, ComplexGraphqlSchema) => ComplexGraphqlSchema.Graph1DbContext.Set<Student>().FirstOrDefault(x => x.Id == m.Id));


        b.ExtendField<Student>("studentsub")
      .ResolveWithService<ComplexGraphqlSchema>((m, ComplexGraphqlSchema) => ComplexGraphqlSchema.Graph1DbContext.Set<StudentDep>().Where(x=>x.StudentId == m.Id))
      .UseFieldPaging();
    };
});

// Add services to the container.

builder.Services.AddControllers();





var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<Graph1DbContext>();
    context.Subjects.AddRange(new[] {
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13375"),
                        Name = "hello1"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("1ac14418-0775-4d44-b888-70f32be73c79"),
                        Name = "hello2"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13376"),
                        Name = "hello3"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13377"),
                        Name = "hello4"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13378"),
                        Name = "hello5"
                    }
                });
    // add student
    context.Students.AddRange(new[] {
                    new Student
                    {
                        Id = Guid.Parse("2229ba81-c75b-4c07-91ca-513760f13375"),
                        Name = "student1"
                    },
                    new Student
                    {
                        Id = Guid.Parse("3339ba81-c75b-4c07-91ca-513760f13375"),
                        Name = "student2"
                    },
                    new Student
                    {
                        Id = Guid.Parse("4449ba81-c75b-4c07-91ca-513760f13375"),
                        Name = "student3"
                    }
                });
    // student dep
    context.StudentDeps.AddRange(new[] {
                    new StudentDep
                    {
                        Id = Guid.Parse("5559ba81-c75b-4c07-91ca-513760f13375"),
                        StudentId = Guid.Parse("2229ba81-c75b-4c07-91ca-513760f13375"),
                        Description = "dep1"
                    },
                    new StudentDep
                    {
                        Id = Guid.Parse("6669ba81-c75b-4c07-91ca-513760f13375"),
                        StudentId = Guid.Parse("2229ba81-c75b-4c07-91ca-513760f13375"),
                        Description = "dep2"
                    },
                    new StudentDep
                    {
                        Id = Guid.Parse("7779ba81-c75b-4c07-91ca-513760f13375"),
                        StudentId = Guid.Parse("3339ba81-c75b-4c07-91ca-513760f13375"),
                        Description = "dep3"
                    }
                });
    context.SaveChanges();
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGraphQL<ComplexGraphqlSchema>();
app.Run();
