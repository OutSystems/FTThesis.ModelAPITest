using OutSystems.Model;
using OutSystems.Model.Logic;
using OutSystems.Model.Logic.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest.ToggleElements
{
    class ClientAction : ElementToggle
    {
        public void GetAllElements(IESpace newe)
        {
            var listactions = newe.GetAllDescendantsOfType<IClientAction>();

            List<IKey> actionKeys = new List<IKey>();
            Console.WriteLine("Client Actions:");
            foreach (IClientAction action in listactions)
            {
                Console.WriteLine(action);
                actionKeys.Add(action.ObjectKey);

            }

            if (actionKeys.Count() != 0)
            {
                InsertIf(newe, actionKeys, "defaultfeature");
            }
        }

        public void GetAllElementsFromList(IESpace newe, List<string> elements, String feature)
        {
            var listactions = newe.GetAllDescendantsOfType<IClientAction>().Where(b => elements.Contains(b.Name)); ;

            List<IKey> actionKeys = new List<IKey>();
            Console.WriteLine("Client Actions:");
            foreach (IClientAction action in listactions.ToList())
            {
                Console.WriteLine(action);
                actionKeys.Add(action.ObjectKey);

            }

            if (actionKeys.Count() != 0)
            {
                InsertIf(newe, actionKeys, feature);
            }
        }

        public void GetDiffElements(IESpace old, IESpace newe, string newOrAltered)
        {
            var listOldClientActions = old.GetAllDescendantsOfType<IClientAction>();

            var listNewClientActions = newe.GetAllDescendantsOfType<IClientAction>();

            List<IClientAction> difActions = new List<IClientAction>();
            List<IKey> difActionKeys = new List<IKey>();

            foreach (IClientAction actions in listNewClientActions)
            {
                var skey = actions.ObjectKey;
                var modDate = ((IFlow)actions).LastModifiedDate;
                if (newOrAltered.Equals("new"))
                {
                    var olds = listOldClientActions.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == null)
                    {
                        difActions.Add(actions);
                        difActionKeys.Add(actions.ObjectKey);
                    }
                }
                else
                {
                    var olds = listOldClientActions.SingleOrDefault(s => (s.ObjectKey.Equals(skey) && ((IFlow)s).LastModifiedDate.Equals(modDate)));
                    var olds2 = listOldClientActions.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == null && olds2 != null)
                    {
                        difActions.Add(actions);
                        difActionKeys.Add(actions.ObjectKey);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Actions:"); }
            else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Actions:"); }

            foreach (IClientAction actions in difActions)
            {
                Console.WriteLine(actions);
            }

            if (newOrAltered.Equals("new"))
            {
                if (difActionKeys.Count() != 0)
                {
                    InsertIf(newe, difActionKeys, "defaultfeature");
                    //CreateScreenPrep(newe, difScreensKeys);
                }
            }
        }

        public void InsertIf(IESpace espace, List<IKey> keys, String feature)
        {
            var actions = espace.GetAllDescendantsOfType<IAction>().Where(s => keys.Contains(s.ObjectKey));
            ToggleEntities t = new ToggleEntities();
            ToggleAction a = new ToggleAction();
            var entity = t.GetTogglesEntity(espace);
            var action = a.GetToggleAction(espace);
            var lib = espace.References.Single(a => a.Name == "FeatureToggle_Lib");
            var getToggleAction = (IServerActionSignature)lib.ServerActions.Single(a => a.Name == "FeatureToggle_IsOn");

            foreach (IAction sa in actions.ToList())
            {
                if (feature == "defaultfeature")
                {
                    feature = sa.Name;
                }
                var rec = t.CreateRecord(entity, $"FT_{espace.Name}_{feature}", $"FT_{feature}", espace);
                var newAction = (IClientAction)espace.Copy(sa);
                var oldname = sa.Name.ToString();
                sa.Name = $"FT_{oldname}";
                newAction.Name = oldname;
                newAction.Public = false;
                var nodes = sa.Nodes;
                foreach (IActionNode n in nodes.ToList())
                {
                    n.Delete();
                }
                foreach (ILocalVariable l in sa.LocalVariables.ToList())
                {
                    l.Delete();
                }
                var start = sa.CreateNode<IStartNode>();
                var getToggle = sa.CreateNode<IExecuteServerActionNode>($"FT_{feature}_IsOn").Below(start);
                var ifToggle = sa.CreateNode<IIfNode>().Below(getToggle);
                var doAction = sa.CreateNode<IExecuteServerActionNode>().ToTheRightOf(ifToggle);
                var end = sa.CreateNode<IEndNode>().Below(doAction);

                getToggle.Action = getToggleAction;
                var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
                getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{espace.Name}_{feature}");
                var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
                getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
                start.Target = getToggle;

                ifToggle.SetCondition($"FT_{feature}_IsOn.IsOn");
                ifToggle.FalseTarget = end;
                getToggle.Target = ifToggle;
                doAction.Action = newAction;
                doAction.Target = end;
                ifToggle.TrueTarget = doAction;
                foreach (IInputParameter i in newAction.InputParameters)
                {
                    doAction.SetArgumentValue(i, i.Name);
                }
                if (newAction.OutputParameters.Count() != 0 )
                {
                    var assign = sa.CreateNode<IAssignNode>().Below(doAction);
                    end.Below(assign);
                    foreach (IOutputParameter o in sa.OutputParameters)
                    {
                        assign.CreateAssignment(o.Name, $"{doAction.Name}.{o.Name}");
                    }
                    
                    doAction.Target = assign;
                    assign.Target = end;
                }

            }

        }
    
    }
}
