﻿using OutSystems.Model;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest
{
    class ScreensToggles : ElementToggles
    {

        private void getDifElements(IESpace old, IESpace newe, String newOrAltered)
        {

            var listOldScreens = old.GetAllDescendantsOfType<IWebScreen>();

            var listNewScreens = newe.GetAllDescendantsOfType<IWebScreen>();

            List<IWebScreen> difScreens = new List<IWebScreen>();
            List<IKey> difScreensKeys = new List<IKey>();

            foreach (IWebScreen screen in listNewScreens)
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

            foreach (IWebScreen screen in difScreens)
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
            var links = espace.GetAllDescendantsOfType<ILinkWidget>().Where(s => screenskeys.Contains(s.OnClick.Destination.ObjectKey));
            var links2 = espace.GetAllDescendantsOfType<ILinkWidget>().Where(s => s.OnClick.Destination is IGoToDestination);
            foreach (ILinkWidget link in links2)
            {
                Console.WriteLine($"LINK: {link}");
                var dest = (IGoToDestination)link.OnClick.Destination;
                if (screenskeys.Contains(dest.Destination.ObjectKey))
                {
                    links = links.Append(link);
                }
            }
            foreach (ILinkWidget l in links)
            {
                Console.WriteLine(l);
                if (l.Parent is IContainerWidget)
                {
                    var parent = (IContainerWidget)l.Parent;
                    var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Web.Widgets.IIfWidget>();
                    instanceIf.SetCondition("True");
                    instanceIf.Name = $"FT_{l.OnClick.Destination.ToString()}";
                    var truebranch = (OutSystems.Model.UI.Web.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                    truebranch.Copy(l);
                    l.Delete();
                }
                else if (l.Parent is OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget)
                {
                    var parent = (OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget)l.Parent;
                    var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Web.Widgets.IIfWidget>();
                    instanceIf.SetCondition("True");
                    instanceIf.Name = $"FT_{l.OnClick.Destination.ToString()}";
                    var truebranch = (OutSystems.Model.UI.Web.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                    truebranch.Copy(l);
                    l.Delete();
                }
                else
                {
                    Console.WriteLine($"Bypass Link {l} because parent is not IPlaceholderContentWidget or IContainerWidget. Parent is {l.Parent}");
                }
            }

        }

        private static void insertIfScreenPrep(IESpace espace, List<IKey> screenskeys)
        {
            var screens = espace.GetAllDescendantsOfType<IWebScreen>().Where(s => screenskeys.Contains(s.ObjectKey));
            foreach (IWebScreen sc in screens)
            {
                Console.WriteLine(sc);
                var preparation = sc.CreatePreparation();
                var start = preparation.CreateNode<IStartNode>();
                var ifToggle = preparation.CreateNode<IIfNode>().Below(start);
                var end = preparation.CreateNode<IEndNode>().Below(ifToggle);

                ifToggle.SetCondition("True");
                ifToggle.TrueTarget = end;
                start.Target = ifToggle;
                var dest = preparation.CreateNode<IDestinationNode>().ToTheRightOf(ifToggle);
                dest.Destination = espace.GetAllDescendantsOfType<IWebScreen>().Single(sr => sr.Name == "InvalidPermissions");
                ifToggle.FalseTarget = dest;
            }

        }
    }
}