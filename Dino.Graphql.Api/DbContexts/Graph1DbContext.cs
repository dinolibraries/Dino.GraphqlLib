﻿using Microsoft.EntityFrameworkCore;

namespace Dino.Graphql.Api.DbContexts
{
    public class Student
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Age { get; set; }
        public IList<StudentDep> Students { get; set; }
    }
    public class Subject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    public class StudentDep
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
    }
    public class Graph1DbContext : DbContext
    {
        public Graph1DbContext(DbContextOptions<Graph1DbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<StudentDep> StudentDeps { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<StudentDep>()
            //    .HasOne(x => x.Student)
            //    .WithMany(x=>x.Students);
        }
    }
}
