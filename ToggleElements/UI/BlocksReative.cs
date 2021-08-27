using OutSystems.Model;
using OutSystems.Model.Logic;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Mobile;
using OutSystems.Model.UI.Mobile.Events;
using OutSystems.Model.UI.Mobile.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest {
    class BlocksReative : BlocksGeneric<IBlock, IMobileBlockInstanceWidget, IMobileScreen, IPlaceholderContentWidget>
    {
        protected override IKey GetObjectKey(IMobileBlockInstanceWidget s)
        {
            return s.SourceBlock.ObjectKey;
        }

        protected override string GetName(IMobileBlockInstanceWidget o)
        {
            return o.SourceBlock.Name.ToString();
        }

        protected override void EncapsulatedInIf(IPlaceholderContentWidget p, IMobileBlockInstanceWidget o, IESpace espace, String feature)
        {
            ToggleManager manager = new ToggleManager();
            var name = GetName(o);
            var actionName = manager.GetFeatureToggleIsOnActionString(feature);
            var outputName = manager.GetFeatureToggleIsOnOutputString(feature);
            var toggleName = manager.GetToggleName(feature);
            var toggleRecord = manager.GetToggleRecord(espace.Name, feature);
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach (IMobileScreen s in screens)
            {
                var exists = s.GetAllDescendantsOfType<IMobileBlockInstanceWidget>().SingleOrDefault(k => k.ObjectKey.Equals(o.ObjectKey));
                if (exists != default)
                {
                    var action = s.GetAllDescendantsOfType<IDataAction>().SingleOrDefault(e => e.Name == "GetToggles");
                    var getToggleAction = manager.GetPlatformToggleRetrievalAction(espace);
                    if (action == default)
                    {
                        var oninitaction = s.CreateDataAction();
                        oninitaction.Name = "GetToggles";
                        var out1 = oninitaction.GetAllDescendantsOfType<IOutputParameter>().SingleOrDefault(o => o.Name == "Out1");
                        out1.Delete();
                        var outputparam = oninitaction.CreateOutputParameter(toggleName);
                        outputparam.DataType = espace.BooleanType;
                        var start = oninitaction.CreateNode<IStartNode>();
                        var getToggle = oninitaction.CreateNode<IExecuteServerActionNode>(actionName).Below(start);
                        var assignVar = oninitaction.CreateNode<IAssignNode>().Below(getToggle);
                        var end = oninitaction.CreateNode<IEndNode>().Below(assignVar);

                        getToggle.Action = getToggleAction;
                        var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
                        getToggle.SetArgumentValue(keyParam, toggleRecord);
                        var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
                        getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
                        start.Target = getToggle;

                        assignVar.CreateAssignment(toggleName, outputName);
                        getToggle.Target = assignVar;
                        assignVar.Target = end;

                    }
                    else
                    {
                        var existsFeature = action.GetAllDescendantsOfType<IExecuteServerActionNode>().SingleOrDefault(s => s.Name == actionName);
                        if (existsFeature == default)
                        {
                            var outputparam = action.CreateOutputParameter(toggleName);
                            outputparam.DataType = espace.BooleanType;
                            var start = action.GetAllDescendantsOfType<IStartNode>().Single();
                            var assign = action.GetAllDescendantsOfType<IAssignNode>().Single();
                            var getToggle = action.CreateNode<IExecuteServerActionNode>(actionName).Below(start);
                            getToggle.Action = getToggleAction;
                            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
                            getToggle.SetArgumentValue(keyParam, toggleRecord);
                            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
                            getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
                            var startTarget = start.Target;
                            assign.CreateAssignment(toggleName, outputName);
                            getToggle.Target = startTarget;
                            start.Target = getToggle;
                        }
                    }
                }
            }
            var instanceIf = p.CreateWidget<IIfWidget>();
            instanceIf.SetCondition($"GetToggles.IsDataFetched and GetToggles.FT_{feature}");
            instanceIf.Name = manager.GetIfWidgetName(feature,name);
            instanceIf.TrueBranch.Copy(o);
        }
    }

}
