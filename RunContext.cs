using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.Applications;
using OutSystems.Model.UI.Mobile;
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
        public static String prefix;
        public static void RunForSpecificFeatures(string[] args)
        {
            string contents = File.ReadAllText(args[1]);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            Console.WriteLine("----------Transformation Report----------");
            var p = deserializer.Deserialize<FeatureSet>(contents);
            /*foreach (Feature f in p.Features)
            {
                Console.WriteLine($"FEATURE: \n{f.Name}");
                foreach (String e in f.Elements)
                {
                    Console.WriteLine(e);
                }
            }*/

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

            if (ESpacePath.Contains(".oml") & saveESpacePath.FullName.Contains(".oml"))
            {
                var module = modelServices.LoadESpace(ESpacePath);
                prefix = module.Name;
                RunTransformation(module, p);
                module.Save(saveESpacePath.FullName);
                //Console.WriteLine($"\nESpace saved to {saveESpacePath.FullName}");
            }
            else if (ESpacePath.Contains(".oap") & saveESpacePath.FullName.Contains(".oap"))
            {
                var app = modelServices.LoadApplication(ESpacePath);
                var modules = app.Modules.ToList();
                foreach(IModuleBytes m  in modules)
                {
                    var module = (IESpace)m.Load();
                    prefix = app.Name;
                    RunTransformation(module, p);
                    module.Save();
                }
                app.Save(saveESpacePath.FullName);
            }
            else
            {
                Console.WriteLine("Input File and Output File must both be modules or applications. They must be of the same file type, either both .oml or both .oap");
            }

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

        private static void RunTransformation(IESpace module, FeatureSet p)
        {
            var isoldtraditional = IsTraditional(module);

            if (isoldtraditional)
            {
                BlocksTraditional traditionalBlocks = new BlocksTraditional();
                ScreensTraditional s = new ScreensTraditional();
                ServerAction l = new ServerAction();
                ToggleRemoteAction t = new ToggleRemoteAction();

                foreach (Feature f in p.Features)
                {
                    Console.WriteLine($"FEATURE: \n{f.Name} : FT_{prefix}_{f.Name}");
                    traditionalBlocks.GetAllElementsFromList(module, f.Elements, f.Name, prefix);
                    s.GetAllElementsFromList(module, f.Elements, f.Name, prefix);
                    l.GetAllElementsFromList(module, f.Elements, f.Name, prefix);
                    Console.WriteLine("-----------------------------------------");
                }
                t.GetToggleAction(module);

            }
            else
            {
                BlocksReative reactiveBlocks = new BlocksReative();
                ScreensReactive s = new ScreensReactive();
                ServerAction l = new ServerAction();
                //ClientAction c = new ClientAction();
                ToggleRemoteAction t = new ToggleRemoteAction();

                foreach (Feature f in p.Features)
                {
                    Console.WriteLine($"FEATURE: \n{f.Name} : FT_{prefix}_{f.Name}");
                    reactiveBlocks.GetAllElementsFromList(module, f.Elements, f.Name, prefix);
                    s.GetAllElementsFromList(module, f.Elements, f.Name, prefix);
                    l.GetAllElementsFromList(module, f.Elements, f.Name, prefix);
                    //c.GetAllElementsFromList(module, f.Elements, f.Name, prefix);
                    Console.WriteLine("-----------------------------------------");
                }
                t.GetToggleAction(module);
            }
            
        }
    }
}
