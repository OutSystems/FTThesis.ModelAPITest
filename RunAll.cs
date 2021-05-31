﻿using OutSystems.Model;
using OutSystems.Model.UI.Web;
using ServiceStudio.Plugin.REST;
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

            var restServices = modelServices.GetPluginService<IRestPluginService>();

            var module = modelServices.LoadESpace(ESpacePath);

            var isoldtraditional = IsTraditional(module);

            if (isoldtraditional)
            {
                BlocksTraditional traditionalBlocks = new BlocksTraditional();
                Screens s = new Screens();
                traditionalBlocks.GetAllElements(module);
                s.GetAllElements(module);
                

            }
            else
            {
                BlocksReative reactiveBlocks = new BlocksReative();
                ScreensNR s = new ScreensNR();
                reactiveBlocks.GetAllElements(module);
                s.GetAllElements(module);
               

            }
            module.Save(saveESpacePath.FullName);
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