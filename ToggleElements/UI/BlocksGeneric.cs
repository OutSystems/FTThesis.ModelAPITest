using OutSystems.Model;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using OutSystems.Model.UI.Mobile.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelAPITest.ToggleElements;

namespace ModelAPITest {
    abstract class BlocksGeneric<GBlock, GObjectSignature, GScreen, GParent> : ElementToggle
        where GBlock : IBlock
        where GObjectSignature: IObjectSignature
        where GScreen : IScreen
        where GParent : IObjectSignature
    {

        public virtual void GetDiffElements(IESpace old, IESpace newe, string newOrAltered) {
            var listOldBlocks = old.GetAllDescendantsOfType<GBlock>();

            var listNewBlocks = newe.GetAllDescendantsOfType<GBlock>();

            List<GBlock> difBlocks = new List<GBlock>();
            List<IKey> difBlocksKeys = new List<IKey>();

            foreach (GBlock block in listNewBlocks) {
                var bkey = block.ObjectKey;
                var modDate = block.LastModifiedDate;
                if (newOrAltered.Equals("new")) {
                    var oldb = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey)));
                    if (oldb == null) {
                        difBlocks.Add(block);
                        difBlocksKeys.Add(block.ObjectKey);
                    }
                } else {
                    var oldb = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey) && s.LastModifiedDate.Equals(modDate)));
                    var oldb2 = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey)));
                    if (oldb == null && oldb2 != null) {
                        difBlocks.Add(block);
                        difBlocksKeys.Add(block.ObjectKey);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("Transformed Blocks:"); } else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Blocks:"); }

            foreach (GBlock block in difBlocks) {
                Console.WriteLine(block);
            }

            if (newOrAltered.Equals("new")) {

                if (difBlocksKeys.Count() != 0) {
                    InsertIf(newe, difBlocksKeys, "defaultfeature", newe.Name);
                }
            }
           
        }

        public virtual void InsertIf(IESpace espace, List<IKey> keys, String feature, string prefix) {
            var bl = espace.GetAllDescendantsOfType<GObjectSignature>().Where(s => keys.Contains(GetObjectKey(s)));
            ToggleEntities t = new ToggleEntities();
            ToggleAction a = new ToggleAction();
            var entity = t.GetTogglesEntity(espace);
            var action = a.GetToggleAction(espace);
            foreach (GObjectSignature o in bl.ToList()) {
                if (o.Parent is GParent) {
                    if (feature == "defaultfeature")
                    {
                        feature = GetName(o);
                    }
                    var parent = (GParent)o.Parent;
                    var rec = t.CreateRecord(entity, $"FT_{prefix}_{feature}", $"FT_{feature}", espace);
                    CreateIf(parent, o, espace, feature, prefix);
                    o.Delete();
                   
                } else {
                    Console.WriteLine($"Bypass Block {o} because parent is not IPlaceholderContentWidget. Parent is {o.Parent}");
                }

            }
            
        }

        protected abstract string GetName(GObjectSignature o);

        protected abstract void CreateIf(GParent p, GObjectSignature o, IESpace espace, String feature, String prefix);

        protected abstract IKey GetObjectKey(GObjectSignature s);

        public void GetAllElements(IESpace newe)
        {
            var listBlocks = newe.GetAllDescendantsOfType<GBlock>();

            List<IKey> difBlocksKeys = new List<IKey>();
            Console.WriteLine("Transformed Blocks:");
            foreach (GBlock block in listBlocks)
            {
                Console.WriteLine(block);
                difBlocksKeys.Add(block.ObjectKey);

            }

            if (difBlocksKeys.Count() != 0)
            {
                InsertIf(newe, difBlocksKeys, "defaultfeature", newe.Name);
            }
        }

        public void GetAllElementsFromList(IESpace newe, List<string> elements, String feature, String prefix)
        {
            
            var listBlocks = newe.GetAllDescendantsOfType<GBlock>().Where(b => elements.Contains(b.Name));

            List<IKey> difBlocksKeys = new List<IKey>();
            Console.WriteLine("Transformed Blocks:");
            foreach (GBlock block in listBlocks)
            {
                Console.WriteLine(block);
                difBlocksKeys.Add(block.ObjectKey);

            }

            if (difBlocksKeys.Count() != 0)
            {
                InsertIf(newe, difBlocksKeys, feature, prefix);
            }
        }
    }
}
