using OutSystems.Model;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest
{
    class BlocksTraditional : BlocksGeneric<IWebBlock, IWebBlockInstanceWidget, IWebScreen>
    {
        protected override IKey GetObjectKey(IWebBlockInstanceWidget s) {
            return s.SourceBlock.ObjectKey;
        }

        protected override string GetName(IWebBlockInstanceWidget o) {
            return o.SourceBlock.Name;
        }

    }
}
