using Dino.Graphql.Api;
using Dino.Graphql.Api.DbContexts;
using Microsoft.EntityFrameworkCore;
using EntityGraphQL.AspNet;
using Dino.GraphqlLib.Infrastructures;
using Dino.Graphql.Api.Mutations;
using Dino.Graphql.Api.Mutations.ModelViews;
using Dino.Graphql.Api.Mutations.DbSelectors;

var builder = WebApplication.CreateBuilder(args);

// add graphql

builder.Services.AddDbContext<Graph1DbContext>(opt => opt.UseInMemoryDatabase("Demo1"));
builder.Services.AddDbContext<Graph2DbContext>(opt => opt.UseInMemoryDatabase("Demo2"));
builder.Services.AddHttpContextAccessor();

builder.Services.AddGraphql<ComplexGraphqlSchema>(builder =>
{
    builder
    .AddFilterExpression<DbContext>()
    .AddWhereClause<Student>(p => x => x.Name.Contains("hello1"))
    .AddWhereClause<Teacher>(p => x => x.Name.Contains("hello3"))
    .AddAuthorizeWhereClause<Subject>((p, opt) =>
    {
        opt.AddRoles(new string[] { "Admin" }, x => x.Name == "hello2");
        opt.AddRoles(new string[] { "User" }, x => x.Name == "hello1");
    });

    builder.FieldBuilder = b =>
    {
        b
        .AddMutationBuilder(typeof(DbContextService<,,>))
        .AddMutation<DbContextSelector1, Subject, SubjectModels.Create, SubjectModels.Update, SubjectModels.Key>()
        .AddMutation<DbContextSelector1, Student, StudentModels.Create, StudentModels.Update, StudentModels.Key>()
        ;
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
    context.SaveChanges();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseGraphql();

app.UseAuthorization();
app.UseRouting();
app.MapControllers();
app.MapGraphQL<ComplexGraphqlSchema>();
app.Run();
