using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.Schema;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using Dino.GraphqlLib.ExpressionHelpers;
using Microsoft.Extensions.Logging;
using Dino.GraphqlLib.Attributes;

namespace Dino.GraphqlLib.Mutations
{
    public class MutationCRUDDedault<TSchemaContext, TModel, TDbSelector, TCreate, TUpdate, TKey> : MutationCRUDBase<TSchemaContext, TModel, TCreate, TUpdate, TKey>
        where TSchemaContext : ISchemaContext
        where TModel : class
        where TCreate : ModelBase<TModel>.CreateBase
        where TUpdate : ModelBase<TModel>.UpdateBase
        where TKey : ModelBase<TModel>.KeyBase
        where TDbSelector : IContextSelector<TSchemaContext>
    {
        protected IDbContextService<TSchemaContext,TModel, TDbSelector> GetService(TSchemaContext schemaContext)
        {
            return schemaContext.Provider.GetService<IDbContextService<TSchemaContext, TModel, TDbSelector>>();
        }
        protected ILogger<TSchemaContext> GetLogger(TSchemaContext schemaContext)
        {
            return schemaContext.Provider.GetService<ILogger<TSchemaContext>>();
        }
        [MutationField]
        public override async Task<Expression<Func<TSchemaContext, TModel>>> CreateAsync(TSchemaContext context, [GraphQLArguments] TCreate entity)
        {
            var logger = GetLogger(context);
            try
            {
                logger?.LogInformation($"Creating {typeof(TModel).Name}");

                var service = GetService(context);
                var modelMapped = entity.MapModelExpression<TCreate, TModel>();
                var result = await service.CreateAsync(modelMapped);

                logger?.LogInformation($"Create {typeof(TModel).Name} successfullly");
                var express =  await service.FindWithIdAsync(result);

                return express;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, ex.Message);
                throw;
            }
        }
        
        [MutationField]
        public override async Task<Expression<Func<TSchemaContext, TModel>>> UpdateAsync(TSchemaContext context, [GraphQLArguments] TKey key, [GraphQLArguments] TUpdate entity)
        {
            var logger = GetLogger(context);
            try
            {
                logger?.LogInformation($"Updating {typeof(TModel).Name}");
                var service = GetService(context);

                var model = await service.FindAsync(key);

                model = entity.MapModelExpression(model);

                var result = await service.UpdateAsync(model);

                logger?.LogInformation($"Update {typeof(TModel).Name} successfullly");
                return await service.FindWithIdAsync(result);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, ex.Message);
                throw;
            }
        }
        [MutationField]
        public override async Task<object> DeleteAsync(TSchemaContext context, [GraphQLArguments] TKey key)
        {
            var logger = GetLogger(context);
            try
            {
                logger?.LogInformation($"Updating {typeof(TModel).Name}");
                var service = GetService(context);

                var model = await service.FindAsync(key);

                var result = await service.DeleteAsync(model);

                logger?.LogInformation($"Update {typeof(TModel).Name} successfullly");
                return result;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, ex.Message);
                throw;
            }
        }

    }
}
