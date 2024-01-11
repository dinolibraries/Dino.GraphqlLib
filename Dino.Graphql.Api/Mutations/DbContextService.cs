using Dino.Graphql.Api.ExpressionHelpers;
using Dino.GraphqlLib.ExpressionHelpers;
using Dino.GraphqlLib.Mutations;
using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dino.Graphql.Api.Mutations
{

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
}
