using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Tests
{
    internal static class Queryhelper
    {
        public const string SubjectQuery = @"query Graph1DbContext {
                graph1DbContext {
                    subject(id: ""8889ba81-c75b-4c07-91ca-513760f13375"") {
                        id
                        name
                    }
                }
            }
            ";
        public const string SubjectPageQuery = @"query Graph1DbContext {
                graph1DbContext {
                    subjects {
                        hasNextPage
                        hasPreviousPage
                        totalItems
                        items {
                            id
                            name
                        }
                    }
                }
            }
            ";


        public const string DoubleQuery = @"query Graph1DbContext {
                                        graph1DbContext {
                                            students(filter: ""age > 3"") {
                                                hasPreviousPage
                                                hasNextPage
                                                totalItems
                                            }
                                        }
                                    }";
        public const string GuidQuery = @"query Graph1DbContext {
    graph1DbContext {
        students(filter: ""id==\""d0c80088-6f32-458e-86eb-2f695d764eb1\"""") {
            hasPreviousPage
            hasNextPage
            totalItems
        }
    }
}
";
        public const string SubjectListQuery = @"query Graph1DbContext {
    graph1DbContext {
        subjects {
            hasPreviousPage
            hasNextPage
            totalItems
        }
    }
}
";
    }
}
