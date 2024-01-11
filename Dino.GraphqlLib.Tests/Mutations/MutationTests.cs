using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Tests.Mutations
{

    public class MutationTests
    {
        [Fact]
        public void TestInnerClass()
        {
            var test = typeof(StudentModel).GetInnnerTypes();
        }
    }
}
