using OutSystems.Model;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Mobile.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest {
    class BlocksReative : BlocksGeneric<IBlock, IMobileBlockInstanceWidget, IScreen> {
        protected override IKey GetObjectKey(IMobileBlockInstanceWidget s) {
            return s.SourceBlock.ObjectKey;
        }

        protected override string GetName(IMobileBlockInstanceWidget o) {
            return o.SourceBlock.Name;
        }

}
