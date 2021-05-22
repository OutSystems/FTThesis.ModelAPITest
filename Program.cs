using System;
using System.IO;
using System.Linq;
using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.Data;
using OutSystems.Model.Logic;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Web;
using ServiceStudio;

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
                ToggleEntities t = new ToggleEntities();
                ToggleAction a = new ToggleAction();
                tradicionalBlocks.GetDiffElements(oldmodule, newmodule, "new");
                s.GetDiffElements(oldmodule, newmodule, "new");
                //var entity = t.GetTogglesEntity(newmodule);
                //t.CreateRecord(entity, "FT_Block3", "Block3Visibility");
                //var action = a.GetToggleAction(newmodule);
                //Console.WriteLine(entity);
                //Console.WriteLine(action);
                /*var lib = newmodule.References.Single(a => a.Name == "FeatureToggle_Lib");
                
                foreach (IServerActionSignature r in lib.ServerActions)
                {
                    Console.WriteLine(r);
                }*/
            }
            else
            {
                BlocksReative reactiveBlocks = new BlocksReative();
                ScreensNR s = new ScreensNR();
                reactiveBlocks.GetDiffElements(oldmodule, newmodule, "new");
                s.GetDiffElements(oldmodule, newmodule, "new");
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

        private static void ListTraditional(IESpace module)
        //only for debugging and experimentation purposes, to be deleted
        {
            BlocksTraditional tradicionalBlocks = new BlocksTraditional();
            tradicionalBlocks.ListBlocksAndScreens(module);
        }

        private static void ListReactive(IESpace module)
        //only for debugging and experimentation purposes, to be deleted
        {
            BlocksReative reactiveBlocks = new BlocksReative();
            reactiveBlocks.ListBlocksAndScreens(module);
        }

    }
}
