using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Extensions.GroupFiltes
{
    public class FieldPagingOffsetArgs
    {
        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}
