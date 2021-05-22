using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutSystems.Model;
using OutSystems.Model.Logic;
using OutSystems.Model.Logic.Nodes;

namespace ModelAPITest.ToggleElements
{
    class ToggleAction
    {
        private const string TogglesEntity = "FeatureToggles";

        public IServerAction GetToggleAction(IESpace espace)
        {
            var action = (IServerAction)espace.ServerActions.SingleOrDefault(s => s.Name == "GetFTValue");
            if (action == default)
            {
                return CreateToggleAction(espace);
            }
            else
            {
                return action;
            }
        }
        public IServerAction CreateToggleAction(IESpace espace)
        {
            var action = espace.CreateServerAction("GetFTValue");
            action.Function = true;

            var ftType = action.CreateInputParameter("FTType");
            ftType.DataType = espace.Entities.Single(s => s.Name == TogglesEntity).IdentifierType; 
            ftType.IsMandatory = true;
            ftType.Description = "Feature Toggle Identifier";

            var isOn = action.CreateOutputParameter("IsOn");
            isOn.DataType = espace.BooleanType;
            isOn.SetDefaultValue("False");
            isOn.Description = "Feature Toggle Value";

            var start = action.CreateNode<IStartNode>();

            var executeaction = action.CreateNode<IExecuteServerActionNode>().Below(start);
            var lib = espace.References.Single(a => a.Name == "FeatureToggle_Lib");
            var getToggleAction = (IServerActionSignature)lib.ServerActions.Single(a => a.Name == "FeatureToggle_IsOn");/////////////////////////////////////////////
            executeaction.Action = getToggleAction;
            var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
            executeaction.SetArgumentValue(keyParam, "FTType");
            var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
            executeaction.SetArgumentValue(modParam, "GetEntryEspaceName()");
            start.Target = executeaction;

            var assign = action.CreateNode<IAssignNode>().Below(executeaction);
            assign.CreateAssignment("IsOn", "FeatureToggle_IsOn.IsOn");
            executeaction.Target = assign;

            var end = action.CreateNode<IEndNode>().Below(assign);
            assign.Target = end;

            return action;
        }
    }
}
