﻿using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest
{
    class ScreensTraditional : ScreensGeneric<ILinkWidget, IWebScreen, IPlaceholderContentWidget, IContainerWidget>
    {
        protected override IObjectSignature GetDestination(ILinkWidget l)
        {
            return l.OnClick.Destination;
        }

        protected override void CreateIf(IPlaceholderContentWidget p, ILinkWidget l, IESpace espace, String feature)
        {
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Web.Widgets.IIfWidget>();
            instanceIf.SetCondition($"GetFTValue(Entities.FeatureToggles.FT_{espace.Name}_{feature})");
            instanceIf.Name = $"If_FT_{feature}_{GetDestinationName(l)}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateIf2(IContainerWidget p, ILinkWidget l, IESpace espace, String feature)
        {
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Web.Widgets.IIfWidget>();
            instanceIf.SetCondition($"GetFTValue(Entities.FeatureToggles.FT_{espace.Name}_{feature})");
            instanceIf.Name = $"If_FT_{feature}_{GetDestinationName(l)}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateScreenPrep(IESpace espace, List<IKey> screenskeys, String feature)
        {
            ToggleEntities t = new ToggleEntities();
            var entity = t.GetTogglesEntity(espace);
            
            var screens = espace.GetAllDescendantsOfType<IWebScreen>().Where(s => screenskeys.Contains(s.ObjectKey));
            foreach (IWebScreen sc in screens)
            {
                if (feature == "defaultfeature")
                {
                    feature = sc.Name;
                }
                var rec = t.CreateRecord(entity, $"FT_{espace.Name}_{feature}", $"FT_{feature}", espace);
                var preparation = sc.CreatePreparation();
                var start = preparation.CreateNode<IStartNode>();
                var ifToggle = preparation.CreateNode<IIfNode>().Below(start);
                var end = preparation.CreateNode<IEndNode>().Below(ifToggle);

                ifToggle.SetCondition($"GetFTValue(Entities.FeatureToggles.FT_{espace.Name}_{feature})");
                ifToggle.TrueTarget = end;
                start.Target = ifToggle;
                var excep = preparation.CreateNode<IRaiseExceptionNode>().ToTheRightOf(ifToggle);
                excep.SetExceptionMessage("\"Screen not available\"");
                excep.Exception = espace.GetAllDescendantsOfType<OutSystems.Model.Logic.IException>().Single(sr => sr.ToString().Contains("Abort Activity Change Exception"));
                ifToggle.FalseTarget = excep;
            }
        }


        protected override IEnumerable<ILinkWidget> InsertIfplus(IESpace espace, List<IKey> keys, IEnumerable<ILinkWidget> links)
        {
            var links2 = espace.GetAllDescendantsOfType<ILinkWidget>().Where(s => s.OnClick.Destination is IGoToDestination);
            foreach (ILinkWidget link in links2)
            {
                var dest = (IGoToDestination)link.OnClick.Destination;
                if (keys.Contains(dest.Destination.ObjectKey))
                {
                    links = links.Append(link);
                }
            }
            return links;
            
        }

        protected override String GetDestinationName(ILinkWidget l)
        {
            return l.OnClick.Destination.ToString().Split(" (", 2)[0];
            
        }
    }
}