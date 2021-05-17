using OutSystems.Model;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelAPITest
{
    class BlocksToggles : ElementToggles
    {
        private void getDifElements(IESpace old, IESpace newe, String newOrAltered)
        {
            var listOldBlocks = old.GetAllDescendantsOfType<IWebBlock>();

            var listNewBlocks = newe.GetAllDescendantsOfType<IWebBlock>();

            List<IWebBlock> difBlocks = new List<IWebBlock>();
            List<IKey> difBlocksKeys = new List<IKey>();

            foreach (IWebBlock block in listNewBlocks)
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

            foreach (IWebBlock block in difBlocks)
            {
                Console.WriteLine(block);
            }

            if (newOrAltered.Equals("new"))
            {
                Console.WriteLine($"Size DifBlocks: {difBlocksKeys.Count()}");
                if (difBlocksKeys.Count() != 0)
                {
                    insertIf(newe, difBlocksKeys);
                }
            }

        }

        private void insertIf(IESpace espace, List<IKey> blockskeys)
        {
            var bl = espace.GetAllDescendantsOfType<IWebBlockInstanceWidget>().Where(s => blockskeys.Contains(s.SourceBlock.ObjectKey));
            foreach (IWebBlockInstanceWidget o in bl)
            {
                if (o.Parent is OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget)
                {
                    var parent = (OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget)o.Parent;
                    var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Web.Widgets.IIfWidget>();
                    instanceIf.SetCondition("True");
                    instanceIf.Name = $"FT_{o.SourceBlock.Name}";
                    var truebranch = (OutSystems.Model.UI.Web.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
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
