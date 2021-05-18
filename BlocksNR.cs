using OutSystems.Model;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Mobile.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest
{
    class BlocksNR : ElementToggle
    {
        public void getDifElements(IESpace old, IESpace newe, string newOrAltered)
        {
            var listOldBlocks = old.GetAllDescendantsOfType<IBlock>();

            var listNewBlocks = newe.GetAllDescendantsOfType<IBlock>();

            List<IBlock> difBlocks = new List<IBlock>();
            List<IKey> difBlocksKeys = new List<IKey>();

            foreach (IBlock block in listNewBlocks)
            {
                var bkey = block.ObjectKey;
                var modDate = block.LastModifiedDate;
                if (newOrAltered.Equals("new"))
                {
                    var oldb = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey)));
                    if (oldb == default)
                    {
                        difBlocks.Add(block);
                        difBlocksKeys.Add(block.ObjectKey);
                    }
                }
                else
                {
                    var oldb = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey) && s.LastModifiedDate.Equals(modDate)));
                    var oldb2 = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey)));
                    if (oldb == default && oldb2 != default)
                    {
                        difBlocks.Add(block);
                        difBlocksKeys.Add(block.ObjectKey);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Blocks:"); }
            else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Blocks:"); }

            foreach (IBlock block in difBlocks)
            {
                Console.WriteLine(block);
            }

            if (newOrAltered.Equals("new"))
            {
                if (difBlocksKeys.Count() != 0)
                {
                    insertIf(newe, difBlocksKeys);
                }
            }
        }

        public void insertIf(IESpace espace, List<IKey> keys)
        {
            var bl = espace.GetAllDescendantsOfType<IMobileBlockInstanceWidget>().Where(s => keys.Contains(s.SourceBlock.ObjectKey));
            foreach (IMobileBlockInstanceWidget o in bl)
            {
                if (o.Parent is OutSystems.Model.UI.Mobile.Widgets.IPlaceholderContentWidget)
                {
                    var parent = (OutSystems.Model.UI.Mobile.Widgets.IPlaceholderContentWidget)o.Parent;
                    var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
                    instanceIf.SetCondition("True");
                    instanceIf.Name = $"FT_{o.SourceBlock.Name}";
                    var truebranch = (OutSystems.Model.UI.Mobile.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                    truebranch.Copy(o);
                    o.Delete();
                }
                else
                {
                    Console.WriteLine($"Bypass Block {o} because parent is not IPlaceholderContentWidget. Parent is {o.Parent}");
                }
            }
        }
    }
}
