using Microsoft.EntityFrameworkCore;

namespace Dino.Graphql.Api.DbContexts
{
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
}
