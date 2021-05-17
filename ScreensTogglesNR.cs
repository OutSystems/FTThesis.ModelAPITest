using OutSystems.Model;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Mobile;
using OutSystems.Model.UI.Mobile.Events;
using ServiceStudio.Plugin.NRWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest
{
    class ScreensTogglesNR : ElementToggles
    {

        private void getDifElements(IESpace old, IESpace newe, String newOrAltered)
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
                Console.WriteLine($"Size DifBlocks: {difScreensKeys.Count()}");
                if (difScreensKeys.Count() != 0)
                {
                    insertIf(newe, difScreensKeys);
                    insertIfScreenPrep(newe, difScreensKeys);
                }
            }
        }

        private void insertIf(IESpace espace, List<IKey> screenskeys)
        {
            var links = espace.GetAllDescendantsOfType<ILink>().Where(s => screenskeys.Contains(s.OnClick.Destination.ObjectKey));

            foreach (ILink l in links)
            {
                if (l.Parent is OutSystems.Model.UI.Mobile.Widgets.IPlaceholderContentWidget)
                {
                    Console.WriteLine($"Here1 link: {l}");
                    Console.WriteLine($"Here1 linkT: {l.GetType()}");
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

        private static void insertIfScreenPrep(IESpace espace, List<IKey> screenskeys)
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
                var dest = oninitaction.CreateNode<IDestinationNode>().ToTheRightOf(ifToggle);
                dest.Destination = espace.GetAllDescendantsOfType<IScreen>().Single(sr => sr.Name == "InvalidPermissions");
                ifToggle.FalseTarget = dest;
                oninit.Destination = oninitaction;
            }

        }
    }
}
