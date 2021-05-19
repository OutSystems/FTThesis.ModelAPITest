using OutSystems.Model;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Mobile;
using OutSystems.Model.UI.Mobile.Events;
using ServiceStudio.Plugin.NRWidgets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelAPITest
{
    
    class ScreensNR : ElementToggle
    {
        public void GetDiffElements(IESpace old, IESpace newe, string newOrAltered)
        {
            var listOldScreens = old.GetAllDescendantsOfType<IScreen>();

            var listNewScreens = newe.GetAllDescendantsOfType<IScreen>();

            List<IScreen> difScreens = new List<IScreen>();
            List<IKey> difScreensKeys = new List<IKey>();

            foreach (IScreen screen in listNewScreens)
            {
                var skey = screen.ObjectKey;
                var modDate = screen.LastModifiedDate;
                if (newOrAltered.Equals("new"))
                {
                    var olds = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == default)
                    {
                        difScreens.Add(screen);
                        difScreensKeys.Add(screen.ObjectKey);
                    }
                }
                else
                {
                    var olds = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey) && s.LastModifiedDate.Equals(modDate)));
                    var olds2 = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == default && olds2 != default)
                    {
                        difScreens.Add(screen);
                        difScreensKeys.Add(screen.ObjectKey);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Screens:"); }
            else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Screens:"); }

            foreach (IScreen screen in difScreens)
            {
                Console.WriteLine(screen);
            }

            if (newOrAltered.Equals("new"))
            {
                if (difScreensKeys.Count() != 0)
                {
                    InsertIf(newe, difScreensKeys);
                    insertIfPrep(newe, difScreensKeys);
                }
            }
        }

        public void InsertIf(IESpace espace, List<IKey> keys)
        {
            var links = espace.GetAllDescendantsOfType<ILink>().Where(s => keys.Contains(s.OnClick.Destination.ObjectKey));

            foreach (ILink l in links)
            {
                if (l.Parent is OutSystems.Model.UI.Mobile.Widgets.IPlaceholderContentWidget)
                {
                    var parent = (OutSystems.Model.UI.Mobile.Widgets.IPlaceholderContentWidget)l.Parent;
                    var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
                    instanceIf.SetCondition("True");
                    instanceIf.Name = $"FT_{l.OnClick.Destination.ToString()}";
                    var truebranch = (OutSystems.Model.UI.Mobile.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                    truebranch.Copy(l);
                    l.Delete();
                }
                else if (l.Parent is OutSystems.Model.UI.Mobile.IContent)
                {
                    var parent = (OutSystems.Model.UI.Mobile.IContent)l.Parent;
                    var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
                    instanceIf.SetCondition("True");
                    instanceIf.Name = $"FT_{l.OnClick.Destination.ToString()}";
                    var truebranch = (OutSystems.Model.UI.Mobile.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                    truebranch.Copy(l);
                    l.Delete();
                }
                else
                {
                    Console.WriteLine($"Bypass Link {l.GetType()} because parent is not IPlaceholderContentWidget. Parent is {l.Parent.GetType()}");
                }
            }
        }


        private static void insertIfPrep(IESpace espace, List<IKey> screenskeys)
        {

            var screens = espace.GetAllDescendantsOfType<IMobileScreen>().Where(s => screenskeys.Contains(s.ObjectKey));

            foreach (IMobileScreen sc in screens)
            {

                var oninit = sc.GetAllDescendantsOfType<IUILifeCycleEvent>().Single(e => e.GetType().ToString().Contains("OnInitialize"));

                var oninitaction = sc.CreateScreenAction();
                oninitaction.Name = "OnInitialize";
                var start = oninitaction.CreateNode<IStartNode>();
                var ifToggle = oninitaction.CreateNode<IIfNode>().Below(start);
                var end = oninitaction.CreateNode<IEndNode>().Below(ifToggle);

                ifToggle.SetCondition("True");
                ifToggle.TrueTarget = end;
                start.Target = ifToggle;
                var excep = oninitaction.CreateNode<IRaiseExceptionNode>().ToTheRightOf(ifToggle);
                excep.SetExceptionMessage("\"Screen not available\"");
                excep.Exception = espace.GetAllDescendantsOfType<OutSystems.Model.Logic.IException>().Single(sr => sr.ToString().Contains("Abort Activity Change Exception"));
                ifToggle.FalseTarget = excep;
                oninit.Destination = oninitaction;
            }

        }
    }
}
