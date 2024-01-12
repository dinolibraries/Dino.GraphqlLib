# Dino.GraphqlLib

## [EntityGraphQL](https://entitygraphql.github.io/) extension library for .NET Core

![Build](https://github.com/lukemurray/EntityGraphQL/actions/workflows/dotnet.yml/badge.svg)

Dino.GraphqlLib extends some additional features:
- Based on roles to be able to horizontally fragment data.
- Add a quick CRUD mutation based on DbContext.
- Does not depend on ef core.

# Get started quickly with 1 ComplexShema and 2 DbContexts

## 1. Define your DbContexts

- Graph1DbContext
```c#
public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
public class Subject
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
public class Graph1DbContext : DbContext
{
    public Graph1DbContext(DbContextOptions<Graph1DbContext> dbContextOptions):base(dbContextOptions) {
    
    }
    public DbSet<Student> Students { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging();
    }
}
```
- Graph2DbContext
```c#
 public class Teacher
 {
     public Guid Id { get; set; }
     public string Name { get; set; }
 }
 public class Graph2DbContext : DbContext
 {
     public Graph2DbContext(DbContextOptions<Graph2DbContext> dbContextOptions):base(dbContextOptions) { }
     public DbSet<Teacher> Teachers { get; set; }
 }
```
## 2. Define  ComplexSchema
```c#
public class ComplexGraphqlSchema : ISchemaContext
{
    public ComplexGraphqlSchema(Graph1DbContext graph1DbContext, Graph2DbContext graph2DbContext, IServiceProvider provider)
    {
        Graph1DbContext = graph1DbContext;
        Graph2DbContext = graph2DbContext;
        Provider = provider;
    }
    public Graph1DbContext Graph1DbContext { get; set; }
    public Graph2DbContext Graph2DbContext { get; set; }
    [GraphQLIgnore]
    public IServiceProvider Provider { get ; set; }
}
```
## 3. Add config to Program.cs
```c#
builder.Services.AddDbContext<Graph1DbContext>(opt => opt.UseInMemoryDatabase("Demo1"));
builder.Services.AddDbContext<Graph2DbContext>(opt => opt.UseInMemoryDatabase("Demo2"));

builder.Services.AddHttpContextAccessor();

builder.Services.AddGraphql<ComplexGraphqlSchema>(builder =>
{
    //horizontal segmentation
    builder
    .AddFilterExpression<DbContext>() // This line will filter all fields that are DbSet in the 2 DbContexts in ComplexSchema and add extentions to it:
                                    //  - UseFilter.
                                    //  - UseOffsetPaging.
                                    //  - UseExpressionFilter (horizontal data fragmentation).
    .AddSiteRoleTransformation(context =>
    {
        return context.Request.Path.StartsWithSegments("admin") ? new[] { "ADMIN" } : null;
    })
    .AddWhereClause<Student>(p => x => x.Name.Contains("hello1"))
    .AddWhereClause<Teacher>(p => x => x.Name.Contains("hello3"))
    .AddAuthorizeWhereClause<Subject>((p, opt) =>
    {
        opt.AddRoles(x => x.RequiresAllRoles("Admin", SiteHelper.GetRoleSite("ADMINSITE")), x => x.Name == "hello2");
        opt.AddRoles(new string[] { "User", SiteHelper.GetRoleSite("ADMINSITE") }, x => x.Name == "hello1");
    });

    // Mutation config
    builder.FieldBuilder = b =>
    {
        b
        .AddMutationBuilder(typeof(DbContextService<,,>))
        .AddMutation<DbContextSelector1, Subject, SubjectModels.Create, SubjectModels.Update, SubjectModels.Key>()
        .AddMutation<DbContextSelector1, Student, StudentModels.Create, StudentModels.Update, StudentModels.Key>()
        ;
    };
});
```
-   Add sitename to ClaimsIdentity.
  ```c#
.AddSiteRoleTransformation(context =>
{
    return context.Request.Path.StartsWithSegments("admin") ? new[] { "ADMIN" } : null;
})
  ```
If the Url starts with **/admin**, then a Role will be added with the name **ADMIN** with the suffix **graph-site** and all will be in lowercase. (**admin-graph-site**)

```c#
var role = SiteHelper.GetRoleSite("ADMIN"); // admin-graph-site
context.User.IsInRole(role); // True
```

## 3. DbContextSelector1.
This is a class that inherits from 
```c#
 public interface IContextSelector<TContext>
 {
     object GetDbContext(TContext context);
 }
```
Its purpose is to select the DbContext during CRUD mutation and compile the query from EXpression.
- Example :
```c#
public class DbContextSelector1 : IContextSelector<ComplexGraphqlSchema>
{
    public object GetDbContext(ComplexGraphqlSchema context)
    {
        return context.Graph1DbContext;
    }
}
```
## 4. DbContextService.
This is a class used to perform CRUD depending on DbContext.
```c#
public class DbContextService<TSchemaContext, TModel, TDbSelector> : IDbContextService<TSchemaContext, TModel, TDbSelector>
    where TSchemaContext : ISchemaContext
    where TModel : class
    where TDbSelector : IContextSelector<TSchemaContext>
{
    private readonly TDbSelector dbSelector;
    private readonly TSchemaContext _ComplexGraphql;
    public DbContextService(IServiceProvider serviceProvider, TSchemaContext complexGraphql)
    {
        dbSelector = ActivatorUtilities.CreateInstance<TDbSelector>(serviceProvider);
        _ComplexGraphql = complexGraphql;
    }
    public DbContext DbContext { get => dbSelector.GetDbContext(_ComplexGraphql) as DbContext; }
    public async Task<TModel> CreateAsync(TModel model)
    {
        var result = await DbContext.AddAsync(model);
        await DbContext.SaveChangesAsync();
        return result.Entity;
    }
    public async Task<object> DeleteAsync(TModel model)
    {
        var result = DbContext.Remove(model);
        await DbContext.SaveChangesAsync();
        return result.Entity;
    }
    public Task<TModel> FindAsync<Tkey>(Tkey key)
    {
        return DbContext.Set<TModel>().Where(DbContext.BuildExpressionFindByIdWith<TModel, Tkey>(key)).FirstOrDefaultAsync();
    }
    public async Task<Expression<Func<TSchemaContext, TModel>>> FindWithIdAsync<Tkey>(Tkey key)
    {
        Expression<Func<TSchemaContext, TModel>> express = (ctx) => DbContext.Set<TModel>().Where(x => x == null).First();
        return await Task.FromResult(express.ReplaceWhereCondition(DbContext.BuildExpressionFindByIdWith<TModel, Tkey>(key)));
    }
    public async Task<TModel> UpdateAsync(TModel model)
    {
        var result = DbContext.Update(model);
        await DbContext.SaveChangesAsync();
        return result.Entity;
    }
}
```
## 5. Add JwtBearer and JWT.io
-   Identity server: 
    -   [JWT.IO](https://jwt.io)
    -   [JWT Authentication](https://manage.auth0.com/)
- Add Configuration :
```c#
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
        //Only development mode.
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
  ```