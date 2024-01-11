using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Tests.Mutations
{
    public class StudentModel : ModelBase<StudentModel>
    {
        public class View : ViewBase { }
        public class Create : CreateBase { }
    }
}
