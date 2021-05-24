using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.Logic.Nodes;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelAPITest
{
    abstract class ScreensGeneric<GLink, GScreen, GParent1, GParent2> : ElementToggle
        where GLink : IObjectSignature
        where GScreen : IScreen
        where GParent1 : IObjectSignature
        where GParent2 : IObjectSignature
    {
        public void GetDiffElements(IESpace old, IESpace newe, string newOrAltered)
        {
            var listOldScreens = old.GetAllDescendantsOfType<GScreen>();

            var listNewScreens = newe.GetAllDescendantsOfType<GScreen>();

            List<GScreen> difScreens = new List<GScreen>();
            List<IKey> difScreensKeys = new List<IKey>();

            foreach (GScreen screen in listNewScreens)
            {
                var skey = screen.ObjectKey;
                var modDate = screen.LastModifiedDate;
                if (newOrAltered.Equals("new"))
                {
                    var olds = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == null)
                    {
                        difScreens.Add(screen);
                        difScreensKeys.Add(screen.ObjectKey);
                    }
                }
                else
                {
                    var olds = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey) && s.LastModifiedDate.Equals(modDate)));
                    var olds2 = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == null && olds2 != null)
                    {
                        difScreens.Add(screen);
                        difScreensKeys.Add(screen.ObjectKey);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Screens:"); }
            else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Screens:"); }

            foreach (GScreen screen in difScreens)
            {
                Console.WriteLine(screen);
            }

            if (newOrAltered.Equals("new"))
            {
                if (difScreensKeys.Count() != 0)
                {
                    InsertIf(newe, difScreensKeys);
                    CreateScreenPrep(newe, difScreensKeys);
                }
            }
        }

        public void InsertIf(IESpace espace, List<IKey> keys)
        {
            var links = espace.GetAllDescendantsOfType<GLink>().Where(s => keys.Contains(GetDestination(s).ObjectKey));
            links = InsertIfplus(espace, keys, links);
            ToggleEntities t = new ToggleEntities();
            ToggleAction a = new ToggleAction(); 
            var entity = t.GetTogglesEntity(espace);
            var action = a.GetToggleAction(espace);
            foreach (GLink l in links)
            {
                if (l.Parent is GParent1)
                {
                    var parent = (GParent1)l.Parent;
                    var rec = t.CreateRecord(entity, $"FT_{espace.Name}_{GetDestinationName(l)}", $"FT_{GetDestinationName(l)}");
                    
                    CreateIf(parent, l, espace);
                }
                else if (l.Parent is GParent2)
                {
                    var parent = (GParent2)l.Parent;
                    var rec =t.CreateRecord(entity, $"FT_{espace.Name}_{GetDestinationName(l)}", $"FT_{GetDestinationName(l)}");
                    
                    CreateIf2(parent, l, espace);
                }
                else
                {
                    Console.WriteLine($"Bypass Link {l} because parent is not IPlaceholderContentWidget or IContainerWidget. Parent is {l.Parent}");
                }
            }
        }

        protected abstract void CreateIf(GParent1 p, GLink o, IESpace eSpace);

        protected abstract void CreateIf2(GParent2 p, GLink o, IESpace eSpace);

        protected abstract void CreateScreenPrep(IESpace espace, List<IKey> screenskeys);

        protected abstract IObjectSignature GetDestination(GLink l);

        protected abstract String GetDestinationName(GLink l);

        protected abstract IEnumerable<GLink> InsertIfplus(IESpace espace, List<IKey> keys, IEnumerable<GLink> links);
    }
}
