using System;
using System.Collections;
using System.Collections.Generic;

namespace Vonalkod
{
    class LakasComparer : IComparer<Lakas>
    {
        public int Compare(Lakas x, Lakas y)
        {
            return String.Compare(String.Join(x.lepcsohaz, x.emeletjelkod, x.ajto, x.ajtotores), String.Join(y.lepcsohaz, y.emeletjelkod, y.ajto, y.ajtotores));
        }
    }
}
