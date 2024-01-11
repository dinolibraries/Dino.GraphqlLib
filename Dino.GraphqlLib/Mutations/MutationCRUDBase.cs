using Dino.GraphqlLib.SchemaContexts;
using Dino.GraphqlLib.Tests.Mutations;
using EntityGraphQL.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
        public abstract Task<object> DeleteAsync(TSchemaContext context, [GraphQLArguments] TKey key);
    }
}
