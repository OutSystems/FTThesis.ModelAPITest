using System;
using System.IO;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI.Web.Widgets;
using OutSystems.Model.UI;
using OutSystems.Model;
using OutSystems.Model.UI.Mobile;
using OutSystems.Model.UI.Mobile.Widgets;
using System.Linq;
using System.Collections.Generic;

namespace ModelAPITest
{
    class Program
    {

        static void Main(string[] args)
        {
            //if (args.Length != 1)
            //{
            //  Console.WriteLine("Usage: <File.oml>");
            //return;
            //}

            Console.WriteLine("Usage: <oldFile.oml>\n<newFile.oml>");
            var oldESpacePath = Console.ReadLine();
            var newESpacePath = Console.ReadLine();

            //var eSpacePath = args[0];
            if (!File.Exists(oldESpacePath))
            {
                Console.WriteLine($"File {oldESpacePath} not found");
                return;
            }
            if (!File.Exists(newESpacePath))
            {
                Console.WriteLine($"File {newESpacePath} not found");
                return;
            }

            var modelServices = OutSystems.ModelAPILoader.Loader.ModelServicesInstance;

            var oldmodule = modelServices.LoadESpace(oldESpacePath);
            var newmodule = modelServices.LoadESpace(newESpacePath);

            var isoldtraditional = isTraditional(oldmodule);
            var isnewtraditional = isTraditional(newmodule);

            if (isoldtraditional != isnewtraditional)
            {
                Console.WriteLine("<oldFile.oml> and <newFile.oml> are not compatible");
                return;
            }

            if (isoldtraditional)
            {
                Console.WriteLine("Old module:");
                listTraditional(oldmodule);
                Console.WriteLine("\nNew module:");
                listTraditional(newmodule);
                getDifScreensTrad(oldmodule, newmodule, "new");
                getDifScreensTrad(oldmodule, newmodule, "altered");
                getDifBlocksTrad(oldmodule, newmodule, "new");
                getDifBlocksTrad(oldmodule, newmodule, "altered");
                insertIfTrad(newmodule);
            }
            else
            {
                Console.WriteLine("Old module:");
                listReactive(oldmodule);
                Console.WriteLine("\nNew module:");
                listReactive(newmodule);
                getDifScreensNR(oldmodule, newmodule, "new");
                getDifScreensNR(oldmodule, newmodule, "altered");
                getDifBlocksNR(oldmodule, newmodule, "new");
                getDifBlocksNR(oldmodule, newmodule, "altered");
                insertIfNR(newmodule);
            }
            //insertIf(newmodule, isoldtraditional);
        }

        private static bool isTraditional(IESpace module)
        {
            var themes = module.GetAllDescendantsOfType<IWebTheme>();
            bool any = false;
            foreach (IWebTheme tm in themes)
            {
                any = true;
            }
            return any;
        }

        private static void listTraditional(IESpace module) {

            var listScreens = module.GetAllDescendantsOfType<IWebScreen>();

            Console.WriteLine("\nScreens:");

            foreach (IWebScreen screen in listScreens)
            {
                Console.WriteLine(screen.Name);
            }

            var listwebblocks = module.GetAllDescendantsOfType<IWebBlock>();

            Console.WriteLine("\nWebBlocks:");

            foreach (IWebBlock block in listwebblocks)
            {
                Console.WriteLine(block.Name);
            }
        }

        private static void listReactive(IESpace module) {

            var listScreens = module.GetAllDescendantsOfType<IScreen>();

            Console.WriteLine("\nScreens:");

            foreach (IScreen screen in listScreens)
            {
                Console.WriteLine(screen);
            }

            var listwebblocks = module.GetAllDescendantsOfType<IBlock>();

            Console.WriteLine("\nWebBlocks:");

            foreach (IBlock block in listwebblocks)
            {
                Console.WriteLine(block);
            }
        }

        private static void getDifScreensTrad(IESpace old, IESpace newe, String newOrAltered) {

            var listOldScreens = old.GetAllDescendantsOfType<IWebScreen>();

            var listNewScreens = newe.GetAllDescendantsOfType<IWebScreen>();

            List<IWebScreen> difScreens = new List<IWebScreen>();

            foreach (IWebScreen screen in listNewScreens)
            {
                var skey = screen.ObjectKey;
                var modDate = screen.LastModifiedDate;
                if (newOrAltered.Equals("new")) {
                    var olds = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == default)
                    {
                        difScreens.Add(screen);
                    }
                }
                else {
                    var olds = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey) && s.LastModifiedDate.Equals(modDate)));
                    var olds2 = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == default && olds2 != default)
                    {
                        difScreens.Add(screen);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Screens:"); }
            else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Screens:"); }

            foreach (IWebScreen screen in difScreens)
            {
                Console.WriteLine(screen);
            }
        }

        private static void getDifScreensNR(IESpace old, IESpace newe, String newOrAltered)
        {
            var listOldScreens = old.GetAllDescendantsOfType<IScreen>();

            var listNewScreens = newe.GetAllDescendantsOfType<IScreen>();

            List<IScreen> difScreens = new List<IScreen>();

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
                    }
                }
                else
                {
                    var olds = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey) && s.LastModifiedDate.Equals(modDate)));
                    var olds2 = listOldScreens.SingleOrDefault(s => (s.ObjectKey.Equals(skey)));
                    if (olds == default && olds2 != default)
                    {
                        difScreens.Add(screen);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Screens:"); }
            else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Screens:"); }

            foreach (IScreen screen in difScreens)
            {
                Console.WriteLine(screen);
            }
        }

        private static void getDifBlocksTrad(IESpace old, IESpace newe, String newOrAltered)
        {
            var listOldBlocks = old.GetAllDescendantsOfType<IWebBlock>();

            var listNewBlocks = newe.GetAllDescendantsOfType<IWebBlock>();

            List<IWebBlock> difBlocks = new List<IWebBlock>();

            foreach (IWebBlock block in listNewBlocks)
            {
                var bkey = block.ObjectKey;
                var modDate = block.LastModifiedDate;
                if(newOrAltered.Equals("new")){
                    var oldb = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey)));
                    if (oldb == default)
                    {
                        difBlocks.Add(block);
                    }
                }
                else{
                    var oldb = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey) && s.LastModifiedDate.Equals(modDate)));
                    var oldb2 = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey)));
                    if (oldb == default && oldb2!=default)
                    {
                        difBlocks.Add(block);
                    }
                }
            }

            if(newOrAltered.Equals("new")){Console.WriteLine("\nNew Blocks:");}
            else if (newOrAltered.Equals("altered")){Console.WriteLine("\nAltered Blocks:");}

            foreach (IWebBlock block in difBlocks)
            {
                Console.WriteLine(block);
            }
            
        }

        private static void getDifBlocksNR(IESpace old, IESpace newe, String newOrAltered)
        {
            var listOldBlocks = old.GetAllDescendantsOfType<IBlock>();

            var listNewBlocks = newe.GetAllDescendantsOfType<IBlock>();

            List<IBlock> difBlocks = new List<IBlock>();

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
                    }
                }
                else
                {
                    var oldb = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey) && s.LastModifiedDate.Equals(modDate)));
                    var oldb2 = listOldBlocks.SingleOrDefault(s => (s.ObjectKey.Equals(bkey)));
                    if (oldb == default && oldb2 != default)
                    {
                        difBlocks.Add(block);
                    }
                }
            }

            if (newOrAltered.Equals("new")) { Console.WriteLine("\nNew Blocks:"); }
            else if (newOrAltered.Equals("altered")) { Console.WriteLine("\nAltered Blocks:"); }

            foreach (IBlock block in difBlocks)
            {
                Console.WriteLine(block);
            }
            
        }

        private static void insertIfTrad(IESpace espace)
        {
            var screens = espace.GetAllDescendantsOfType<IWebScreen>();
            foreach (IWebScreen sr in screens)
            {
                if(sr.Name.Equals("WebScreen2"))
                {
                    var bl = sr.GetAllDescendantsOfType<IWebBlockInstanceWidget>();
                    Console.WriteLine("\nInstanceWidgets:");
                    foreach (IWebBlockInstanceWidget o in bl)
                    {
                        if(o.SourceBlock.Name == "WebBlock3")
                        {
                            if(o.Parent is OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget)
                            {
                                var parent = (OutSystems.Model.UI.Web.Widgets.IPlaceholderContentWidget)o.Parent;
                                var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Web.Widgets.IIfWidget>();
                                instanceIf.SetCondition("True");
                                instanceIf.Name = "FT_WebBlock3";
                                var truebranch = (OutSystems.Model.UI.Web.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                                var trueblock = truebranch.CreateWidget<IWebBlockInstanceWidget>();
                                trueblock.SourceBlock = o.SourceBlock;
                                o.Delete(); 
                            }
                            else
                            {
                                Console.WriteLine("Bypass");
                            }
                        }
                    }
                    espace.Save(@"C:\Users\blo\Desktop\SimpleScreensTranformed.oml");
                }
            }
        }

        private static void insertIfNR(IESpace espace)
        {
            var screens = espace.GetAllDescendantsOfType<IScreen>();
            
            foreach (IScreen sr in screens)
            {
                if (sr.Name.Equals("Screen2"))
                {
                    var bl = sr.GetAllDescendantsOfType<IMobileBlockInstanceWidget>();
                    Console.WriteLine("\nInstanceWidgets:");
                    foreach (IMobileBlockInstanceWidget o in bl)
                    {
                        Console.WriteLine(o);
                        Console.WriteLine(o.SourceBlock);
                        if (o.SourceBlock.Name == "Block3")
                        {
                            if (o.Parent is OutSystems.Model.UI.Mobile.Widgets.IPlaceholderContentWidget)
                            {
                                var parent = (OutSystems.Model.UI.Mobile.Widgets.IPlaceholderContentWidget)o.Parent;
                                var instanceIf = parent.CreateWidget<OutSystems.Model.UI.Mobile.Widgets.IIfWidget>();
                                instanceIf.SetCondition("True");
                                instanceIf.Name = "FT_Block3";
                                var truebranch = (OutSystems.Model.UI.Mobile.Widgets.IIfBranchWidget)instanceIf.TrueBranch;
                                var trueblock = truebranch.CreateWidget<IMobileBlockInstanceWidget>();
                                trueblock.SourceBlock = o.SourceBlock;
                                o.Delete();
                            }
                            else
                            {
                                Console.WriteLine("Bypass");
                            }
                        }
                    }
                    espace.Save(@"C:\Users\blo\Desktop\SimpleScreensTranformedNR.oml");
                }
            }
        }

    }


}
