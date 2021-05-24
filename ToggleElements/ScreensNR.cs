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
using OutSystems.Model.Logic;
using OutSystems.Model.Types;

namespace ModelAPITest
{

    class ScreensNR : ScreensGeneric<ILink, IMobileScreen, IPlaceholderContentWidget, IContent>
    {
        protected override IObjectSignature GetDestination(ILink l)
        {
            return l.OnClick.Destination;
        }

        protected override void CreateIf(IPlaceholderContentWidget p, ILink l, IESpace espace)
        {
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach(IMobileScreen s in screens)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    var localvar = s.CreateLocalVariable($"FT_{GetDestinationName(l)}");
                    localvar.DataType = espace.BooleanType;
                    CreateOnInitializeScreen(espace, false, s, l);
                }
            }

            var blocks = espace.GetAllDescendantsOfType<IMobileBlock>();
            foreach (IMobileBlock s in blocks)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    var localvar = s.CreateLocalVariable($"FT_{GetDestinationName(l)}");
                    localvar.DataType = espace.BooleanType;
                    CreateOnInitializeBlock(espace, false, s, l);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"FT_{GetDestinationName(l)}");
            instanceIf.Name = $"FT_{GetDestinationName(l)}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateIf2(IContent p, ILink l, IESpace espace)
        {
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach (IMobileScreen s in screens)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    var localvar = s.CreateLocalVariable($"FT_{GetDestinationName(l)}");
                    localvar.DataType = espace.BooleanType;
                    CreateOnInitializeScreen(espace, false, s, l);
                }
            }
            var blocks = espace.GetAllDescendantsOfType<IMobileBlock>();
            foreach (IMobileBlock s in blocks)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    var localvar = s.CreateLocalVariable($"FT_{GetDestinationName(l)}");
                    localvar.DataType = espace.BooleanType;
                    CreateOnInitializeBlock(espace, false, s, l);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"FT_{GetDestinationName(l)}");
            instanceIf.Name = $"FT_{GetDestinationName(l)}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateScreenPrep(IESpace espace, List<IKey> screenskeys)
        {
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>().Where(s => screenskeys.Contains(s.ObjectKey));

            foreach (IMobileScreen sc in screens)
            {
                CreateOnInitializeScreen(espace, true, sc, null);
            }
        }

        private void CreateOnInitializeScreen(IESpace espace, Boolean isPrep, IMobileScreen sc, ILink l)
        {
            var oninit = sc.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnInitialize"));

            var oninitaction = sc.CreateScreenAction();
            //var act = sc.CreateDataAction();
            oninitaction.Name = "OnInitialize";
            CreateOnInitialize(espace, isPrep, sc.Name, l, oninitaction);
            oninit.Destination = oninitaction;
        }
        private void CreateOnInitializeBlock(IESpace espace, Boolean isPrep, IMobileBlock sc, ILink l)
        {
            var oninit = sc.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnInitialize"));

            var oninitaction = sc.CreateScreenAction();
            //var act = sc.CreateDataAction();
            oninitaction.Name = "OnInitialize";
            CreateOnInitialize(espace, isPrep, sc.Name, l, oninitaction);
            oninit.Destination = oninitaction;
        }
        private void CreateOnInitialize(IESpace espace, Boolean isPrep, String name, ILink l, IScreenAction oninitaction)
        {
            var start = oninitaction.CreateNode<IStartNode>();
            var getToggle = oninitaction.CreateNode<IExecuteServerActionNode>().Below(start);
            var end = oninitaction.CreateNode<IEndNode>();

            var lib = espace.References.Single(a => a.Name == "FeatureToggle_Lib");
            var getToggleAction = (IServerActionSignature)lib.ServerActions.Single(a => a.Name == "FeatureToggle_IsOn");/////////////////////////////////////////////
            getToggle.Action = getToggleAction;
            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
            getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
            start.Target = getToggle;
            if (isPrep)
            {
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{name}");
                var ifToggle = oninitaction.CreateNode<IIfNode>().Below(getToggle);
                ifToggle.SetCondition("FeatureToggle_IsOn.IsOn");
                end.Below(ifToggle);
                ifToggle.TrueTarget = end;
                getToggle.Target = ifToggle;
                var excep = oninitaction.CreateNode<IRaiseExceptionNode>().ToTheRightOf(ifToggle);
                excep.SetExceptionMessage("\"Screen not available\"");
                excep.Exception = espace.GetAllDescendantsOfType<OutSystems.Model.Logic.IException>().Single(sr => sr.ToString().Contains("Abort Activity Change Exception"));
                ifToggle.FalseTarget = excep;
            }
            else
            {
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{GetDestinationName(l)}");
                var assignVar = oninitaction.CreateNode<IAssignNode>().Below(getToggle);
                assignVar.CreateAssignment($"FT_{GetDestinationName(l)}", "FeatureToggle_IsOn.IsOn");
                getToggle.Target = assignVar;
                end.Below(assignVar);
                assignVar.Target = end;
            }
            
        }

        protected override IEnumerable<ILink> InsertIfplus(IESpace espace, List<IKey> keys, IEnumerable<ILink> links)
        {
            return links;
        }

        protected override string GetDestinationName(ILink l)
        {
            var screen = (IScreen)l.OnClick.Destination;
            return screen.Name;
        }
    }
}
