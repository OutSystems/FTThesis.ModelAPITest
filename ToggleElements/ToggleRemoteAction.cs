using OutSystems.Model;
using OutSystems.Model.Data;
using OutSystems.Model.Logic;
using OutSystems.Model.Logic.Aggregates;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.Processes;
using ServiceStudio.Plugin.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest.ToggleElements
{
    class ToggleRemoteAction
    {
        public IAction GetToggleAction(IESpace espace)
        {
            var action = (IServerAction)espace.ServerActions.SingleOrDefault(s => s.Name == "CreateTogglesRemote");
            if (action == default)
            {
                return CreateToggleRemoteAction(espace);
            }
            else
            {
                return action;
            }

        }

        public IAction CreateToggleRemoteAction(IESpace espace)
        {
            var action = espace.CreateServerAction("CreateTogglesRemote");
            action.Function = false;
            ConstructAction(espace, action);
            var timer = espace.CreateTimer("CreateTogglesPlugin");
            CreateTimer(espace, timer, action);
            return action;
        }

        private void ConstructAction(IESpace espace, IAction action)
        {
            var start = action.CreateNode<IStartNode>();
            var toggleslist = action.CreateNode<IAggregateNode>("GetFeatureToggles").Below(start); ;
            var cycle = action.CreateNode<IForEachNode>().Below(toggleslist);
            var assign = action.CreateNode<IAssignNode>().ToTheRightOf(cycle);
            var createaction = action.CreateNode<IExecuteServerActionNode>().Below(assign);
            var end = action.CreateNode<IEndNode>().Below(cycle); ;

            start.Target = toggleslist;
            toggleslist.Target = cycle;
            cycle.CycleTarget = assign;
            assign.Target = createaction;
            createaction.Target = cycle;
            cycle.Target = end;

            var entity = (IStaticEntity)espace.Entities.SingleOrDefault(s => s.Name == "FeatureToggles");
            var datatable = toggleslist.AsDatabaseAggregate.CreateSource(entity);
            cycle.SetRecordList("GetFeatureToggles.List");

            var res = espace.GetAllDescendantsOfType<IRestClient>().SingleOrDefault(a => a.Name == "FeatureToggleManagementAPI");
            var createToggleAction = res.Actions.Single(a => a.Name == "CreateFeatureToggle");
            createaction.Action = createToggleAction;

            var args = createaction.Arguments.Single();

            var localvar = action.CreateLocalVariable("ToggleDefinition");
            localvar.DataType = espace.GetAllDescendantsOfType<IRestStructure>().Single(s => s.Name == "APIFeatureToggleCreateRequest");

            assign.CreateAssignment("ToggleDefinition.FeatureToggle.Key", "GetFeatureToggles.List.Current.FeatureToggles.Key");
            assign.CreateAssignment("ToggleDefinition.FeatureToggle.Name", "GetFeatureToggles.List.Current.FeatureToggles.Label");
            assign.CreateAssignment("ToggleDefinition.FeatureToggle.IsUnderDevelopment", "True");
            assign.CreateAssignment("ToggleDefinition.FeatureToggle.IsActiveByDefault", "False");

            assign.CreateAssignment("ToggleDefinition.FeatureToggleMetadata.FeatureToggleId", "GetFeatureToggles.List.Current.FeatureToggles.Key");
            assign.CreateAssignment("ToggleDefinition.FeatureToggleMetadata.Owner", "GetUserId()");
            assign.CreateAssignment("ToggleDefinition.FeatureToggleMetadata.IsTemporary", "True");
            assign.CreateAssignment("ToggleDefinition.FeatureToggleMetadata.PredictedActivationDate", "CurrDate()");

            args.SetValue(localvar.Name);

        }

        private void CreateTimer(IESpace espace, ITimer timer, IAction action)
        {
            timer.Schedule = "WhenPublished";
            timer.Priority = OutSystems.Model.Enumerations.Priority.High;
            timer.Action = action;
        }
    }
}
