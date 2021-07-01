﻿using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.UI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModelAPITest
{
    class RunContext
    {
        public static void RunForSpecificFeatures(string[] args)
        {
            string contents = File.ReadAllText(args[1]);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var p = deserializer.Deserialize<FeatureSet>(contents);
            foreach (Feature f in p.Features)
            {
                Console.WriteLine($"Feature: {f.Name}, Elements:");
                foreach (String e in f.Elements)
                {
                    Console.WriteLine(e);
                }
            }

            var ESpacePath = args[2];

            if (!File.Exists(ESpacePath))
            {
                Console.WriteLine($"File {ESpacePath} not found");
                return;
            }

            var saveESpacePath = new FileInfo(args[3]);
            var outputDirectory = saveESpacePath.Directory.FullName;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var modelServices = OutSystems.ModelAPILoader.Loader.ModelServicesInstance;

            var module = modelServices.LoadESpace(ESpacePath);

            var isoldtraditional = IsTraditional(module);

            if (isoldtraditional)
            {
                BlocksTraditional traditionalBlocks = new BlocksTraditional();
                Screens s = new Screens();
                ServerAction l = new ServerAction();
                //ToggleRemoteAction t = new ToggleRemoteAction();
                foreach (Feature f in p.Features)
                {
                    traditionalBlocks.GetAllElementsFromList(module, f.Elements, f.Name);
                    s.GetAllElementsFromList(module, f.Elements, f.Name);
                    l.GetAllElementsFromList(module, f.Elements, f.Name);
                }
                //t.GetToggleAction(module);
                
            }
            else
            {
                BlocksReative reactiveBlocks = new BlocksReative();
                ScreensNR s = new ScreensNR();
                ServerAction l = new ServerAction();
                //ClientAction c = new ClientAction();
                //ToggleRemoteAction t = new ToggleRemoteAction();
                foreach (Feature f in p.Features)
                {
                    reactiveBlocks.GetAllElementsFromList(module, f.Elements, f.Name);
                    s.GetAllElementsFromList(module, f.Elements, f.Name);
                    l.GetAllElementsFromList(module, f.Elements, f.Name);
                    //c.GetAllElementsFromList(module, f.Elements, f.Name);
                }
                //t.GetToggleAction(module);
                

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