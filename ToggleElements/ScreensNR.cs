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
            var name = GetDestinationName(l);
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach(IMobileScreen s in screens)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    var localvar = s.CreateLocalVariable($"FT_{name}");
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
                    var localvar = s.CreateLocalVariable($"FT_{name}");
                    localvar.DataType = espace.BooleanType;
                    CreateOnInitializeBlock(espace, false, s, l);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"FT_{name}");
            instanceIf.Name = $"If_FT_{name}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateIf2(IContent p, ILink l, IESpace espace)
        {
            var name = GetDestinationName(l);
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach (IMobileScreen s in screens)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    var localvar = s.CreateLocalVariable($"FT_{name}");
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
                    var localvar = s.CreateLocalVariable($"FT_{name}");
                    localvar.DataType = espace.BooleanType;
                    CreateOnInitializeBlock(espace, false, s, l);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"FT_{name}");
            instanceIf.Name = $"If_FT_{name}";
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
            IScreenAction action = (IScreenAction)oninit.Destination;
            if (action == null)
            {
                var oninitaction = sc.CreateScreenAction();
                oninitaction.Name = "OnInitialize";
                CreateOnInitialize(espace, isPrep, sc.Name, l, oninitaction);
                oninit.Destination = oninitaction;
            }
            else
            {
                AddToOnInitialize(espace, isPrep, sc.Name, l, action);
            }
        }
        private void CreateOnInitializeBlock(IESpace espace, Boolean isPrep, IMobileBlock sc, ILink l)
        {
            var oninit = sc.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnInitialize"));
            IScreenAction action = (IScreenAction)oninit.Destination;
            if(action == null)
            {
                var oninitaction = sc.CreateScreenAction();
                oninitaction.Name = "OnInitialize";
                CreateOnInitialize(espace, isPrep, sc.Name, l, oninitaction);
                oninit.Destination = oninitaction;
            }
            else
            {
                AddToOnInitialize(espace, isPrep, sc.Name, l, action);
            }
            
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
                getToggle.Name = $"FT_{name}_IsOn";
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{name}");
                var ifToggle = oninitaction.CreateNode<IIfNode>().Below(getToggle);
                ifToggle.SetCondition($"FT_{name}_IsOn.IsOn");
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
                var destname = GetDestinationName(l);
                getToggle.Name = $"FT_{destname}_IsOn";
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{destname}");
                var assignVar = oninitaction.CreateNode<IAssignNode>().Below(getToggle);
                assignVar.CreateAssignment($"FT_{destname}", $"FT_{destname}_IsOn.IsOn");
                getToggle.Target = assignVar;
                end.Below(assignVar);
                assignVar.Target = end;
            }
            
        }

        private void AddToOnInitialize(IESpace espace, Boolean isPrep, String name, ILink l, IScreenAction action)
        { 
            var lib = espace.References.Single(a => a.Name == "FeatureToggle_Lib");
            var getToggleAction = (IServerActionSignature)lib.ServerActions.Single(a => a.Name == "FeatureToggle_IsOn");
            var start = action.GetAllDescendantsOfType<IStartNode>().Single();
            var assign = action.GetAllDescendantsOfType<IAssignNode>().Single();

            var getToggle = action.CreateNode<IExecuteServerActionNode>().Below(start);
            getToggle.Action = getToggleAction;
            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
            getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
            var startTarget = start.Target;
            if (isPrep)
            {
                getToggle.Name = $"FT_{name}_IsOn";
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{name}");
                var ifToggle = action.CreateNode<IIfNode>().Below(getToggle);
                ifToggle.SetCondition($"FT_{name}_IsOn.IsOn");
                getToggle.Target = ifToggle;
                var excep = action.CreateNode<IRaiseExceptionNode>().ToTheRightOf(ifToggle);
                excep.SetExceptionMessage("\"Screen not available\"");
                excep.Exception = espace.GetAllDescendantsOfType<OutSystems.Model.Logic.IException>().Single(sr => sr.ToString().Contains("Abort Activity Change Exception"));
                ifToggle.FalseTarget = excep;
                ifToggle.TrueTarget = startTarget;
            }
            else
            {
                var destname = GetDestinationName(l);
                getToggle.Name = $"FT_{destname}_IsOn";
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{destname}");
                if (assign != null)
                {
                    assign.CreateAssignment($"FT_{destname}", $"FT_{destname}_IsOn.IsOn");
                    getToggle.Target = startTarget;
                }
                else
                {
                    assign = action.CreateNode<IAssignNode>().Below(getToggle);
                    assign.CreateAssignment($"FT_{destname}", $"FT_{destname}_IsOn.IsOn");
                    getToggle.Target = assign;
                    assign.Target = startTarget;
                }
            }
            start.Target = getToggle;
        }

        protected override IEnumerable<ILink> InsertIfplus(IESpace espace, List<IKey> keys, IEnumerable<ILink> links)
        {
            return links;
        }

        protected override string GetDestinationName(ILink l)
        {
            var screen = (IScreen)l.OnClick.Destination;
            return screen.Name.ToString();
        }
    }
}
