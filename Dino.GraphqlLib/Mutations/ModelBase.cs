using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    public class ModelBase<TModel>
        where TModel : class
    {
        public abstract class ViewBase : ModelBase<TModel> { }
        public abstract class CreateBase : ModelBase<TModel> { }
        public abstract class UpdateBase : ModelBase<TModel> { }
        public abstract class KeyBase : ModelBase<TModel> { }
    }
}
