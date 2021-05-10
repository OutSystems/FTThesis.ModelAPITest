using System;
using System.IO;
using OutSystems.Model.UI.Web;
using OutSystems.Model.UI;
using OutSystems.Model;
using System.Linq;
using System.Collections.Generic;

namespace ModelAPITest
{
    class Program
    {

        private const string TWSimpleScreensESpacePath = @"..\..\..\SimpleScreensTW.oml";

        private const string NRSimpleScreensESpacePath = @"..\..\..\SimpleScreensNR.oml";

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
            }
            else
            {
                Console.WriteLine("Old module:");
                listReactive(oldmodule);
                Console.WriteLine("\nNew module:");
                listReactive(newmodule);
            }

            getDifScreens(oldmodule, newmodule, isoldtraditional, "new");
            getDifScreens(oldmodule, newmodule, isoldtraditional, "altered");
            getDifBlocks(oldmodule, newmodule, isoldtraditional, "new");
            getDifBlocks(oldmodule, newmodule, isoldtraditional, "altered");

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

        private static void listTraditional(IESpace module){
        
             var listScreens = module.GetAllDescendantsOfType<IWebScreen>();

            Console.WriteLine("Screens:");

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

        private static void listReactive(IESpace module){
        
             var listScreens = module.GetAllDescendantsOfType<IScreen>();

            Console.WriteLine("Screens:");

            foreach (IScreen screen in listScreens)
            {
                Console.WriteLine(screen.Name);
            }

            var listwebblocks = module.GetAllDescendantsOfType<IBlock>();

            Console.WriteLine("\nWebBlocks:");

            foreach (IBlock block in listwebblocks)
            {
                Console.WriteLine(block.Name);
            }
        }

        private static void getDifScreens(IESpace old, IESpace newe, bool isTrad, String newOrAltered){

            if (isTrad) {
                var listOldScreens = old.GetAllDescendantsOfType<IWebScreen>();

                var listNewScreens = newe.GetAllDescendantsOfType<IWebScreen>();

                List<IWebScreen> difScreens = new List<IWebScreen>();

                foreach (IWebScreen screen in listNewScreens)
                {
                    var skey = screen.ObjectKey;
                    var modDate = screen.LastModifiedDate;
                    if(newOrAltered.Equals("new")){
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

                if(newOrAltered.Equals("new")){Console.WriteLine("\nNew Screens:");}
                else if (newOrAltered.Equals("altered")){Console.WriteLine("\nAltered Screens:");}

                foreach (IWebScreen screen in difScreens)
                {
                    Console.WriteLine(screen);
                }

            }
            else
            {
                var listOldScreens = old.GetAllDescendantsOfType<IScreen>();

                var listNewScreens = newe.GetAllDescendantsOfType<IScreen>();

                List<IScreen> difScreens = new List<IScreen>();

                foreach (IScreen screen in listNewScreens)
                {
                    var skey = screen.ObjectKey;
                    var modDate = screen.LastModifiedDate;
                    if(newOrAltered.Equals("new")){
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

                 if(newOrAltered.Equals("new")){Console.WriteLine("\nNew Screens:");}
                else if (newOrAltered.Equals("altered")){Console.WriteLine("\nAltered Screens:");}

                foreach (IScreen screen in difScreens)
                {
                    Console.WriteLine(screen);
                }
            }


        }

        private static void getDifBlocks(IESpace old, IESpace newe, bool isTrad, String newOrAltered)
        {

            if (isTrad)
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
            else
            {
                var listOldBlocks = old.GetAllDescendantsOfType<IBlock>();

                var listNewBlocks = newe.GetAllDescendantsOfType<IBlock>();

                List<IBlock> difBlocks = new List<IBlock>();

                foreach (IBlock block in listNewBlocks)
                {
                    var bkey = block.GlobalKey;
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
                        if (oldb == default && oldb2 != default)
                        {
                            difBlocks.Add(block);
                        }
                    }
                    

                }

                if(newOrAltered.Equals("new")){Console.WriteLine("\nNew Blocks:");}
                else if (newOrAltered.Equals("altered")){Console.WriteLine("\nAltered Blocks:");}

                foreach (IBlock block in difBlocks)
                {
                    Console.WriteLine(block);
                }
            }


        }


    }
}
