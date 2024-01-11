using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    public interface IDbContextService<TSchemaContext, TModel, TDbSelector>
        where TDbSelector : IContextSelector<TSchemaContext>
        where TModel : class
    {
        Task<TModel> CreateAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task<object> DeleteAsync(TModel model);
        Task<TModel> FindAsync<Tkey>(Tkey key);
        //Expression<Func<TSource, bool>> BuildExpressionFindById<TSource>(TSource source);
        //Expression<Func<TSource, bool>> BuildExpressionFindByIdWith<TSource, Tkey>(Tkey valueObject);
        /// <summary>
        /// Expression<Func<TDContext, TModel>>
        /// </summary>
        /// <typeparam name="TTKey"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Expression<Func<TSchemaContext, TModel>>> FindWithIdAsync<Tkey>(Tkey key);
        /// 
    }

}
