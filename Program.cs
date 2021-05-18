using System;
using System.IO;
using OutSystems.Model;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Web;

namespace ModelAPITest
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: <oldFile.oml> <newFile.oml> <outputFile.oml>");
                return;
            }

            var oldESpacePath = args[0];
            var newESpacePath = args[1];

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

            var saveESpacePath = new FileInfo(args[2]);
            var outputDirectory = saveESpacePath.Directory.FullName;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
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
                Blocks b = new Blocks();
                Screens s = new Screens();
                b.getDifElements(oldmodule, newmodule, "new");
                s.getDifElements(oldmodule, newmodule, "new");
            }
            else
            {
                BlocksNR b = new BlocksNR();
                ScreensNR s = new ScreensNR();
                b.getDifElements(oldmodule, newmodule, "new");
                s.getDifElements(oldmodule, newmodule, "new");
            }
            newmodule.Save(saveESpacePath.FullName);
            Console.WriteLine($"\nESpace saved to {saveESpacePath.FullName}");
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

        private static void listTraditional(IESpace module) 
        //only for debugging and experimentation purposes, to be deleted
        {

            var listScreens = module.GetAllDescendantsOfType<IWebScreen>();

            Console.WriteLine("\nScreens:");

            foreach (IWebScreen screen in listScreens)
            {
                Console.WriteLine(screen);
            }

            var listwebblocks = module.GetAllDescendantsOfType<IWebBlock>();

            Console.WriteLine("\nWebBlocks:");

            foreach (IWebBlock block in listwebblocks)
            {
                Console.WriteLine(block.Name);
            }
        }

        private static void listReactive(IESpace module)
        //only for debugging and experimentation purposes, to be deleted
        {
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

    }
}
