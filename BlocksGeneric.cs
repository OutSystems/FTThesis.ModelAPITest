using OutSystems.Model;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest {
    abstract class BlocksGeneric<GBlock, GObjectSignature, GScreen> : ElementToggle
        where GBlock : IBlock
        where GObjectSignature: IObjectSignature
        where GScreen : IScreen {

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

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Blocks:"); } else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Blocks:"); }

            foreach (GBlock block in difBlocks) {
                Console.WriteLine(block);
            }

            if (newOrAltered.Equals("new")) {

                if (difBlocksKeys.Count() != 0) {
                    InsertIf(newe, difBlocksKeys);
                }
            }
        }

        public virtual void InsertIf(IESpace espace, List<IKey> keys) {
            var bl = espace.GetAllDescendantsOfType<GObjectSignature>().Where(s => keys.Contains(GetObjectKey(s)));
            foreach (GObjectSignature o in bl) {
                if (o.Parent is OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget) {
                    var parent = (OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget)o.Parent;
                    var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Web.Widgets.IIfWidget>();
                    instanceIf.SetCondition("True");
                    instanceIf.Name = $"FT_{GetName(o)}";
                    var truebranch = (OutSystems.Model.UI.Web.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                    truebranch.Copy(o);
                    o.Delete();
                } else {
                    Console.WriteLine($"Bypass Block {o} because parent is not IPlaceholderContentWidget. Parent is {o.Parent}");
                }

            }
        }

        /// <summary>
        /// only for debugging and experimentation purposes, to be deleted
        /// </summary>
        /// <param name="module"></param>
        public void ListBlocksAndScreens(IESpace module) {
            var listScreens = module.GetAllDescendantsOfType<GScreen>();

            Console.WriteLine("\nScreens:");

            foreach (GScreen screen in listScreens) {
                Console.WriteLine(screen);
            }

            var listwebblocks = module.GetAllDescendantsOfType<GBlock>();

            Console.WriteLine("\nWebBlocks:");

            foreach (GBlock block in listwebblocks) {
                Console.WriteLine(block);
            }
        }

        protected abstract string GetName(GObjectSignature o);

        protected abstract IKey GetObjectKey(GObjectSignature s);
    }
}
