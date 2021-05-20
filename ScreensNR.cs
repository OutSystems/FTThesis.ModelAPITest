using OutSystems.Model;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Mobile;
using OutSystems.Model.UI.Mobile.Widgets;
using OutSystems.Model.UI.Mobile.Events;
using ServiceStudio.Plugin.NRWidgets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelAPITest
{

    class ScreensNR : ScreensGeneric<ILink, IMobileScreen, IPlaceholderContentWidget, IContent>
    {
        protected override IObjectSignature GetDestination(ILink l)
        {
            return l.OnClick.Destination;
        }

        protected override void CreateIf(IPlaceholderContentWidget p, ILink l)
        {
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition("True");
            instanceIf.Name = $"FT_{l.OnClick.Destination.ToString()}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateIf2(IContent p, ILink l)
        {
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition("True");
            instanceIf.Name = $"FT_{l.OnClick.Destination.ToString()}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateScreenPrep(IESpace espace, List<IKey> screenskeys)
        {
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>().Where(s => screenskeys.Contains(s.ObjectKey));

            foreach (IMobileScreen sc in screens)
            {

                var oninit = sc.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnInitialize"));

                var oninitaction = sc.CreateScreenAction();
                oninitaction.Name = "OnInitialize";
                var start = oninitaction.CreateNode<IStartNode>();
                var ifToggle = oninitaction.CreateNode<IIfNode>().Below(start);
                var end = oninitaction.CreateNode<IEndNode>().Below(ifToggle);

                ifToggle.SetCondition("True");
                ifToggle.TrueTarget = end;
                start.Target = ifToggle;
                var excep = oninitaction.CreateNode<IRaiseExceptionNode>().ToTheRightOf(ifToggle);
                excep.SetExceptionMessage("\"Screen not available\"");
                excep.Exception = espace.GetAllDescendantsOfType<OutSystems.Model.Logic.IException>().Single(sr => sr.ToString().Contains("Abort Activity Change Exception"));
                ifToggle.FalseTarget = excep;
                oninit.Destination = oninitaction;
            }
        }

        protected override IEnumerable<ILink> InsertIfplus(IESpace espace, List<IKey> keys, IEnumerable<ILink> links)
        {
            return links;
        }
    }
}
