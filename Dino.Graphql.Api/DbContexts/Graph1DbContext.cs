using Microsoft.EntityFrameworkCore;

namespace Dino.Graphql.Api.DbContexts
{
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
}
