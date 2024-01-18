using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.Schema;
using System.Linq.Expressions;

namespace Dino.GraphqlLib.Mutations
{
    public interface IMutationCRUD
    {

    }
    public interface IMutationCRUD<TModel> : IMutationCRUD
        where TModel : class
    {

    }
    public abstract class MutationCRUDBase<TSchemaContext, TModel, TCreate, TUpdate, TKey> : IMutationCRUD<TModel>
        where TSchemaContext : ISchemaContext
        where TModel : class
        where TCreate : ModelBase<TModel>.CreateBase
        where TUpdate : ModelBase<TModel>.UpdateBase
        where TKey : ModelBase<TModel>.KeyBase
    {
        public abstract Task<Expression<Func<TSchemaContext, TModel>>> CreateAsync(TSchemaContext context, [GraphQLArguments] TCreate entity);
        public abstract Task<Expression<Func<TSchemaContext, TModel>>> UpdateAsync(TSchemaContext context, [GraphQLArguments] TKey key, [GraphQLArguments] TUpdate entity);
        public abstract Task<TModel> DeleteAsync(TSchemaContext context, [GraphQLArguments] TKey key);
    }
}
