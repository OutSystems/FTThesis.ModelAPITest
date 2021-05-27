using OutSystems.Model;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest
{
    class BlocksTraditional : BlocksGeneric<IWebBlock, IWebBlockInstanceWidget, IWebScreen, IPlaceholderContentWidget>
    {
        protected override IKey GetObjectKey(IWebBlockInstanceWidget s) {
            return s.SourceBlock.ObjectKey;
        }

        protected override string GetName(IWebBlockInstanceWidget o) {
            return o.SourceBlock.Name;
        }

        protected override void CreateIf(IPlaceholderContentWidget p, IWebBlockInstanceWidget o, IESpace eSpace)
        {
            var instanceIf = p.CreateWidget<IIfWidget>();
            instanceIf.SetCondition($"GetFTValue(Entities.FeatureToggles.FT_{eSpace.Name}_{GetName(o)})");
            instanceIf.Name = $"If_FT_{GetName(o)}";
            instanceIf.TrueBranch.Copy(o);
        }
    }
}
