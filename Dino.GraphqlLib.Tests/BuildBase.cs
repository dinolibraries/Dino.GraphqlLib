using Dino.Graphql.Api.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Tests
{
    public class BuildBase
    {
        public BuildBase()
        {
        }

        public class ServiceOption
        {
            public string[] Roles { get; set; } = new string[0];
            public string Name { get; set; }
        }
        protected virtual IServiceProvider SetupSercvice(ServiceOption serviceOption)
        {

            var services = ServiceHelper.GetServiceCollection(builder =>
            {
                builder
                .AddFilterExpression<DbContext>()
                .AddSiteRoleTransformation(context =>
                {
                    return new[] { context.GetGraphqlSite() };
                })
                .AddAuthorizeWhereClause<Subject>((option) =>
                {
                    option.AddRoles(requird => requird.RequiresAllRoles(serviceOption.Roles), p => x => x.Name == serviceOption.Name);
                    option.AddRoles(requird => requird.RequiresAllRoles(RoleHelper.Manage), p => x => x.Name == serviceOption.Name);
                });
            });


            var provider = services.BuildServiceProvider();
            InitialDbContext(provider);
            return provider;
        }
        protected virtual IServiceProvider SetupSercvice()
        {
            var services = ServiceHelper.GetServiceCollection(builder =>
            {

                builder
                .AddFilterExpression<DbContext>()
                ;

            });

            services.AddHttpContextAccessor();

            var provider = services.BuildServiceProvider();
            InitialDbContext(provider);
            return provider;
        }
        static bool IsInital = false;
        protected virtual void InitialDbContext(IServiceProvider provider)
        {
            var _context1 = provider.GetService<Graph1DbContext>();
            var _context2 = provider.GetService<Graph2DbContext>();

            if (!_context1.Subjects.Any() && !IsInital)
            {
                IsInital = true;
                _context1.Subjects.AddRange(new[] {
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
                _context1.SaveChanges();

            }
        }

    }
}
