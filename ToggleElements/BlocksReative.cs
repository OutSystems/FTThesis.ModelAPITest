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
            return o.SourceBlock.Name;
        }

        protected override void CreateIf(IPlaceholderContentWidget p, IMobileBlockInstanceWidget o, IESpace espace)
        {
            Console.WriteLine("HERE7");
            var screens = espace.GetAllDescendantsOfType<IMobileScreen>();
            foreach (IMobileScreen s in screens)
            {
                Console.WriteLine("HERE8");
                Console.WriteLine(s);
                Console.WriteLine(o);
                Console.WriteLine(o.ObjectKey);
                var existss = s.GetAllDescendantsOfType<IMobileBlockInstanceWidget>();
                foreach (IMobileBlockInstanceWidget k in existss)
                {
                    Console.WriteLine($"Desc: {k}, {k.ObjectKey}");
                }
                var exists = s.GetAllDescendantsOfType<IMobileBlockInstanceWidget>().SingleOrDefault(k => k.ObjectKey.Equals(o.ObjectKey));
                Console.WriteLine(exists);
                if (exists != default)
                {
                    Console.WriteLine("HERE9");
                    var localvar = s.CreateLocalVariable($"FT_{GetName(o)}");
                    localvar.DataType = espace.BooleanType;
                    var oninit = s.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnInitialize"));

                    var oninitaction = s.CreateScreenAction();
                    oninitaction.Name = "OnInitialize";
                    var start = oninitaction.CreateNode<IStartNode>();
                    var getToggle = oninitaction.CreateNode<IExecuteServerActionNode>().Below(start);
                    var assignVar = oninitaction.CreateNode<IAssignNode>().Below(getToggle);
                    var end = oninitaction.CreateNode<IEndNode>().Below(assignVar);

                    var lib = espace.References.Single(a => a.Name == "FeatureToggle_Lib");
                    var getToggleAction = (IServerActionSignature)lib.ServerActions.Single(a => a.Name == "FeatureToggle_IsOn");/////////////////////////////////////////////
                    getToggle.Action = getToggleAction;
                    var keyParam = getToggleAction.InputParameters.Single(s => s.Name == "FeatureToggleKey");
                    getToggle.SetArgumentValue(keyParam, $"Entities.FeatureToggles.FT_{GetName(o)}");
                    var modParam = getToggleAction.InputParameters.Single(s => s.Name == "ModuleName");
                    getToggle.SetArgumentValue(modParam, "GetEntryEspaceName()");
                    start.Target = getToggle;

                    assignVar.CreateAssignment($"FT_{GetName(o)}", "FeatureToggle_IsOn.IsOn");
                    getToggle.Target = assignVar;
                    assignVar.Target = end;

                    oninit.Destination = oninitaction;
                }
            }
            var instanceIf = p.CreateWidget<IIfWidget>();
            instanceIf.SetCondition($"FT_{GetName(o)}");
            instanceIf.Name = $"FT_{GetName(o)}";
            instanceIf.TrueBranch.Copy(o);
        }
    }

}
