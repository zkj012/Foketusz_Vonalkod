using System;
using System.Collections;
using System.Collections.Generic;

namespace Vonalkod
{
    class LakasComparer : IComparer<Lakas>
    {
        public int Compare(Lakas x, Lakas y)
        {
            return String.Compare(String.Join(x.lepcsohaz, x.emelet, x.ajto, x.ajtotores), String.Join(y.lepcsohaz, y.emelet, y.ajto, y.ajtotores));
        }
    }
}
