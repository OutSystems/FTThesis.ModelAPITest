using OutSystems.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelAPITest
{
    interface ElementToggle
    {
        void GetDiffElements(IESpace old, IESpace newe, String newOrAltered);

        void InsertIf(IESpace espace, List<IKey> keys);
    }
}
