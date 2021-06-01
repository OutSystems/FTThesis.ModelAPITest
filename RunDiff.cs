using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.UI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModelAPITest
{
    class RunDiff
    {
        public static void RunForDiffElements(string[] args)
        {
            var oldESpacePath = args[1];
            var newESpacePath = args[2];

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

            var saveESpacePath = new FileInfo(args[3]);
            var outputDirectory = saveESpacePath.Directory.FullName;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var modelServices = OutSystems.ModelAPILoader.Loader.ModelServicesInstance;

            var oldmodule = modelServices.LoadESpace(oldESpacePath);
            var newmodule = modelServices.LoadESpace(newESpacePath);

            var isoldtraditional = IsTraditional(oldmodule);
            var isnewtraditional = IsTraditional(newmodule);

            if (isoldtraditional != isnewtraditional)
            {
                Console.WriteLine("<oldFile.oml> and <newFile.oml> are not compatible");
                return;
            }

            if (isoldtraditional)
            {
                BlocksTraditional tradicionalBlocks = new BlocksTraditional();
                Screens s = new Screens();
                ServerAction l = new ServerAction();
                tradicionalBlocks.GetDiffElements(oldmodule, newmodule, "new");
                s.GetDiffElements(oldmodule, newmodule, "new");
                l.GetDiffElements(oldmodule, newmodule, "new");


            }
            else
            {
                BlocksReative reactiveBlocks = new BlocksReative();
                ScreensNR s = new ScreensNR();
                ServerAction l = new ServerAction();
                reactiveBlocks.GetDiffElements(oldmodule, newmodule, "new");
                s.GetDiffElements(oldmodule, newmodule, "new");
                l.GetDiffElements(oldmodule, newmodule, "new");
            }

            newmodule.Save(saveESpacePath.FullName);
            Console.WriteLine($"\nESpace saved to {saveESpacePath.FullName}");
        }

        private static bool IsTraditional(IESpace module)

        {
            var themes = module.GetAllDescendantsOfType<IWebTheme>();
            bool any = false;
            foreach (IWebTheme tm in themes)
            {
                any = true;
            }
            return any;
        }
    }
}
