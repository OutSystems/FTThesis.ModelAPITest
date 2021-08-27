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

    class ScreensReactive : ScreensGeneric<ILink, IMobileScreen, IPlaceholderContentWidget, IContent>
    {
        protected override IObjectSignature GetDestination(ILink l)
        {
            return l.OnClick.Destination;
        }

        protected override void EncapsulatedInIf(IPlaceholderContentWidget p, ILink l, IESpace espace, String feature)
        {
            ToggleManager manager = new ToggleManager();
            var name = GetDestinationName(l);
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach(IMobileScreen s in screens)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    CreateDataActionScreen(espace, s, l, feature);
                }
            }

            var blocks = espace.GetAllDescendantsOfType<IMobileBlock>();
            foreach (IMobileBlock s in blocks)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    CreateDataActionBlock(espace, s, l, feature);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"GetToggles.IsDataFetched and GetToggles.FT_{feature}");
            instanceIf.Name = manager.GetIfWidgetName(feature,name);
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void EncapsulatedInIf2(IContent p, ILink l, IESpace espace, String feature)
        {
            ToggleManager manager = new ToggleManager();
            var name = GetDestinationName(l);
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach (IMobileScreen s in screens)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    CreateDataActionScreen(espace, s, l, feature);
                }
            }
            var blocks = espace.GetAllDescendantsOfType<IMobileBlock>();
            foreach (IMobileBlock s in blocks)
            {
                var exists = s.GetAllDescendantsOfType<ILink>().SingleOrDefault(k => k.ObjectKey.Equals(l.ObjectKey));
                if (exists != default)
                {
                    CreateDataActionBlock(espace, s, l, feature);
                }
            }
            var instanceIf = p.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
            instanceIf.SetCondition($"GetToggles.IsDataFetched and GetToggles.FT_{feature}");
            instanceIf.Name = manager.GetIfWidgetName(feature, name);
            instanceIf.TrueBranch.Copy(l);
            l.Delete();
        }

        protected override void CreateScreenPrep(IESpace espace, List<IKey> screenskeys, String feature)
        {
            ToggleManager manager = new ToggleManager();

            var screens = espace.GetAllDescendantsOfType<IMobileScreen>().Where(s => screenskeys.Contains(s.ObjectKey));

            foreach (IMobileScreen sc in screens)
            {
                if (feature == "defaultfeature")
                {
                    feature = sc.Name;
                }
                var rec = manager.CreateToggleRecord(manager.GetToggleKey(espace.Name,feature), manager.GetToggleName(feature), espace);
                var dataaction = CreateDataActionScreen(espace, sc, null, feature);
                var ongetdata = dataaction.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnAfterFetch"));
                IScreenAction action = (IScreenAction)ongetdata.Destination;
                var oninitaction = sc.CreateScreenAction();
                oninitaction.Name = "OnAfterFetch";

                var start = oninitaction.CreateNode<IStartNode>();
                var end = oninitaction.CreateNode<IEndNode>();
                var name = sc.Name;

                var ifToggle = oninitaction.CreateNode<IIfNode>().Below(start);
                ifToggle.SetCondition($"GetToggles.FT_{feature}");
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

        private IDataAction CreateDataActionScreen(IESpace espace, IMobileScreen sc, ILink l, String feature)
        {
            var action = sc.GetAllDescendantsOfType<IDataAction>().SingleOrDefault(e => e.Name == "GetToggles");

            if (action == default)
            {
                action = sc.CreateDataAction();
                action.Name = "GetToggles";
                var out1 = action.GetAllDescendantsOfType<IOutputParameter>().SingleOrDefault(o => o.Name == "Out1");
                out1.Delete();
                CreateDataAction(espace, l, action, sc.Name, feature);
            }
            else
            {
                AddToDataAction(espace, l, action, sc.Name, feature);
            }
            return action;
        }

        private IDataAction CreateDataActionBlock(IESpace espace, IMobileBlock sc, ILink l, String feature)
        {
            var action = sc.GetAllDescendantsOfType<IDataAction>().SingleOrDefault(e => e.Name == "GetToggles");

            if (action == default)
            {
                action = sc.CreateDataAction();
                action.Name = "GetToggles";
                var out1 = action.GetAllDescendantsOfType<IOutputParameter>().SingleOrDefault(o => o.Name == "Out1");
                out1.Delete();
                CreateDataAction(espace, l, action, sc.Name, feature);
            }
            else
            {
                
                AddToDataAction(espace, l, action, sc.Name, feature);
                
            }
            return action;
        }

        private void CreateDataAction(IESpace espace, ILink l, IDataAction oninitaction, String sname, String feature)
        {
            ToggleManager manager = new ToggleManager();

            var start = oninitaction.CreateNode<IStartNode>();
            var getToggle = oninitaction.CreateNode<IExecuteServerActionNode>().Below(start);
            var end = oninitaction.CreateNode<IEndNode>();

            var getToggleAction = manager.GetPlatformToggleRetrievalAction(espace);
            getToggle.Action = getToggleAction;
            start.Target = getToggle;

            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
            var destname = sname;
            if(l != null)
            {
                destname = GetDestinationName(l);
            }
            if (destname != feature)
            {
                destname = feature;
            }

            var toggleName = manager.GetToggleName(destname);
            var toggleRecord = manager.GetToggleRecord(espace.Name, destname);
            var outputName = manager.GetFeatureToggleIsOnOutputString(destname);

            getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
            getToggle.SetArgumentValue(keyParam, toggleRecord);
            getToggle.Name = manager.GetFeatureToggleIsOnActionString(destname);

            var outputparam = oninitaction.CreateOutputParameter(toggleName);
            outputparam.DataType = espace.BooleanType;

            var assignVar = oninitaction.CreateNode<IAssignNode>().Below(getToggle);
            assignVar.CreateAssignment(toggleName, outputName);

            getToggle.Target = assignVar;
            end.Below(assignVar);
            assignVar.Target = end;
            
        }

        private void AddToDataAction(IESpace espace, ILink l, IDataAction action, String sname, String feature)
        {
            ToggleManager manager = new ToggleManager();
            var getToggleAction = manager.GetPlatformToggleRetrievalAction(espace);
            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
            var start = action.GetAllDescendantsOfType<IStartNode>().Single();
            var assign = action.GetAllDescendantsOfType<IAssignNode>().Single();
            var destname = sname;
            if (l != null)
            {
                destname = GetDestinationName(l);
            }
            if (destname != feature)
            {
                destname = feature;
            }
            var toggleName = manager.GetToggleName(destname);
            var toggleRecord = manager.GetToggleRecord(espace.Name, destname);
            var outputName = manager.GetFeatureToggleIsOnOutputString(destname);
            var actionName = manager.GetFeatureToggleIsOnActionString(destname);

            var startTarget = start.Target;
            var existsFeature = action.GetAllDescendantsOfType<IExecuteServerActionNode>().SingleOrDefault(s => s.Name == actionName);
            if (existsFeature == default)
            {
                var getToggle = action.CreateNode<IExecuteServerActionNode>().Below(start);
                getToggle.Action = getToggleAction;
                getToggle.SetArgumentValue(keyParam, toggleRecord);
                getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
                getToggle.Name = actionName;

                var outputparam = action.CreateOutputParameter(toggleName);
                outputparam.DataType = espace.BooleanType;

                if (assign != null)
                {
                    assign.CreateAssignment(toggleName, outputName);
                    getToggle.Target = startTarget;
                }
                else
                {
                    assign = action.CreateNode<IAssignNode>().Below(getToggle);
                    assign.CreateAssignment(toggleName, outputName);
                    getToggle.Target = assign;
                    assign.Target = startTarget;
                }

                start.Target = getToggle;
            }
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
