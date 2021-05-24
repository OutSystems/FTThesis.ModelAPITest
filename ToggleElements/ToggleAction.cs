using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutSystems.Model;
using OutSystems.Model.Logic;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI.Web;

namespace ModelAPITest.ToggleElements
{
    class ToggleAction
    {
        private const string TogglesEntity = "FeatureToggles";

        public IAction GetToggleAction(IESpace espace)
        {
            
            if (IsTraditional(espace))
            {
                var action = (IServerAction)espace.ServerActions.SingleOrDefault(s => s.Name == "GetFTValue");
                if (action == default)
                {
                    return CreateToggleAction(espace, true);
                }
                else
                {
                    return action;
                }
            }
            else
            {
                return null;
            }
        }
        public IAction CreateToggleAction(IESpace espace, Boolean isTrad)
        {
           
           
                var action = espace.CreateServerAction("GetFTValue");
                action.Function = true;
                ConstructAction(espace, action);
                return action;
            
           
            
        }

        private void ConstructAction(IESpace espace, IAction action)
        {
           
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
        }

        private static bool IsTraditional(IESpace module)

        {
            var themes = module.GetAllDescendantsOfType<IWebTheme>();
            bool any = false;
            foreach (IWebTheme tm in themes)
            {
                any = true;
            }
            return any;
        }
    }
}
