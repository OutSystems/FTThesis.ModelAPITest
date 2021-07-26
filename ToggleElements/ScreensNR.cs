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
using ModelAPITest.ToggleElements;

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
                    CreateDataActionScreen(espace, s, l);
                }
            }

            var blocks = espace.GetAllDescendantsOfType<IMobileBlock>();
            foreach (IMobileBlock s in blocks)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    CreateDataActionBlock(espace, s, l);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"GetToggles.IsDataFetched and GetToggles.FT_{name}");
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
                    CreateDataActionScreen(espace, s, l);
                }
            }
            var blocks = espace.GetAllDescendantsOfType<IMobileBlock>();
            foreach (IMobileBlock s in blocks)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    CreateDataActionBlock(espace, s, l);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"GetToggles.IsDataFetched and GetToggles.FT_{name}");
            instanceIf.Name = $"If_FT_{name}";
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateScreenPrep(IESpace espace, List<IKey> screenskeys)
        {
            ToggleEntities t = new ToggleEntities();
            var entity = t.GetTogglesEntity(espace);

            var screens = espace.GetAllDescendantsOfType<IMobileScreen>().Where(s => screenskeys.Contains(s.ObjectKey));

            foreach (IMobileScreen sc in screens)
            {
                var rec = t.CreateRecord(entity, $"FT_{espace.Name}_{sc.Name}", $"FT_{sc.Name}", espace);
                var dataaction = CreateDataActionScreen(espace, sc, null);
                var ongetdata = sc.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnAfterFetch"));
                IScreenAction action = (IScreenAction)ongetdata.Destination;
                var oninitaction = sc.CreateScreenAction();
                oninitaction.Name = "OnAfterFetch";

                var start = oninitaction.CreateNode<IStartNode>();
                //var getToggle = oninitaction.CreateNode<IExecuteServerActionNode>().Below(start);
                var end = oninitaction.CreateNode<IEndNode>();
                var name = sc.Name;

                /*getToggle.Action = getToggleAction;
                getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
                start.Target = getToggle;
                getToggle.Name = $"FT_{name}_IsOn";
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{espace.Name}_{name}");*/

                var ifToggle = oninitaction.CreateNode<IIfNode>().Below(start);
                ifToggle.SetCondition($"GetToggles.FT_{name}");
                end.Below(ifToggle);
                ifToggle.TrueTarget = end;
                start.Target = ifToggle;

                var excep = oninitaction.CreateNode<IRaiseExceptionNode>().ToTheRightOf(ifToggle);
                excep.SetExceptionMessage("\"Screen not available\"");
                excep.Exception = espace.GetAllDescendantsOfType<OutSystems.Model.Logic.IException>().Single(sr => sr.ToString().Contains("Abort Activity Change Exception"));
                ifToggle.FalseTarget = excep;
                ongetdata.Destination = oninitaction;
            }
        }

        private IDataAction CreateDataActionScreen(IESpace espace, IMobileScreen sc, ILink l)
        {
            var action = sc.GetAllDescendantsOfType<IDataAction>().SingleOrDefault(e => e.Name == "GetToggles");

            if (action == default)
            {
                action = sc.CreateDataAction();
                action.Name = "GetToggles";
                var out1 = action.GetAllDescendantsOfType<IOutputParameter>().SingleOrDefault(o => o.Name == "Out1");
                out1.Delete();
                CreateDataAction(espace, l, action, sc.Name);
            }
            else
            {
                AddToDataAction(espace, l, action, sc.Name);
            }
            return action;
        }

        private IDataAction CreateDataActionBlock(IESpace espace, IMobileBlock sc, ILink l)
        {
            var action = sc.GetAllDescendantsOfType<IDataAction>().SingleOrDefault(e => e.Name == "GetToggles");

            if (action == default)
            {
                action = sc.CreateDataAction();
                action.Name = "GetToggles";
                var out1 = action.GetAllDescendantsOfType<IOutputParameter>().SingleOrDefault(o => o.Name == "Out1");
                out1.Delete();
                CreateDataAction(espace, l, action, sc.Name);
            }
            else
            {
                AddToDataAction(espace, l, action, sc.Name);
            }
            return action;
        }

        private void CreateDataAction(IESpace espace, ILink l, IDataAction oninitaction, String sname)
        {
            var start = oninitaction.CreateNode<IStartNode>();
            var getToggle = oninitaction.CreateNode<IExecuteServerActionNode>().Below(start);
            var end = oninitaction.CreateNode<IEndNode>();

            var lib = espace.References.Single(a => a.Name == "FeatureToggle_Lib");
            var getToggleAction = (IServerActionSignature)lib.ServerActions.Single(a => a.Name == "FeatureToggle_IsOn");
            getToggle.Action = getToggleAction;
            start.Target = getToggle;

            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
            var destname = sname;
            if(l != null)
            {
                destname = GetDestinationName(l);
            }
            getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
            getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{espace.Name}_{destname}");
            getToggle.Name = $"FT_{destname}_IsOn";

            var outputparam = oninitaction.CreateOutputParameter($"FT_{destname}");
            outputparam.DataType = espace.BooleanType;

            var assignVar = oninitaction.CreateNode<IAssignNode>().Below(getToggle);
            assignVar.CreateAssignment($"FT_{destname}", $"FT_{destname}_IsOn.IsOn");

            getToggle.Target = assignVar;
            end.Below(assignVar);
            assignVar.Target = end;
            
        }

        private void AddToDataAction(IESpace espace, ILink l, IDataAction action, String sname)
        { 
            var lib = espace.References.Single(a => a.Name == "FeatureToggle_Lib");
            var getToggleAction = (IServerActionSignature)lib.ServerActions.Single(a => a.Name == "FeatureToggle_IsOn");
            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
            var start = action.GetAllDescendantsOfType<IStartNode>().Single();
            var assign = action.GetAllDescendantsOfType<IAssignNode>().Single();
            var destname = sname;
            if (l != null)
            {
                destname = GetDestinationName(l);
            }
            var startTarget = start.Target;

            var getToggle = action.CreateNode<IExecuteServerActionNode>().Below(start);
            getToggle.Action = getToggleAction;
            getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{espace.Name}_{destname}");
            getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
            getToggle.Name = $"FT_{destname}_IsOn";
            
            var outputparam = action.CreateOutputParameter($"FT_{destname}");
            outputparam.DataType = espace.BooleanType;

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
