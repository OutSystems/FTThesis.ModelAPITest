using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.UI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModelAPITest
{
    class RunAll
    {
        public static void RunForAllElements(string[] args)
        {
            var ESpacePath = args[1];

            if (!File.Exists(ESpacePath))
            {
                Console.WriteLine($"File {ESpacePath} not found");
                return;
            }

            var saveESpacePath = new FileInfo(args[2]);
            var outputDirectory = saveESpacePath.Directory.FullName;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var modelServices = OutSystems.ModelAPILoader.Loader.ModelServicesInstance;

            var module = modelServices.LoadESpace(ESpacePath);

            var isoldtraditional = IsTraditional(module);

            Console.WriteLine("----------Transformation Report----------");

            if (isoldtraditional)
            {
                BlocksTraditional traditionalBlocks = new BlocksTraditional();
                ScreensTraditional s = new ScreensTraditional();
                ServerAction l = new ServerAction();
                traditionalBlocks.GetAllElements(module);
                s.GetAllElements(module);
                l.GetAllElements(module);
                ToggleRemoteAction t = new ToggleRemoteAction();
                t.GetToggleAction(module);
            }
            else
            {
                BlocksReative reactiveBlocks = new BlocksReative();
                ScreensReactive s = new ScreensReactive();
                ServerAction l = new ServerAction();
                //ClientAction c = new ClientAction();
                reactiveBlocks.GetAllElements(module);
                s.GetAllElements(module);
                l.GetAllElements(module);
                //c.GetAllElements(module);
                ToggleRemoteAction t = new ToggleRemoteAction();
                t.GetToggleAction(module);

            }
            module.Save(saveESpacePath.FullName);
            //Console.WriteLine($"\nESpace saved to {saveESpacePath.FullName}");
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

