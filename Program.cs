using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModelAPITest.ToggleElements;
using OutSystems.Model;
using OutSystems.Model.Data;
using OutSystems.Model.Logic;
using OutSystems.Model.UI;
using OutSystems.Model.UI.Mobile.Widgets;
using OutSystems.Model.UI.Web;
using ServiceStudio;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModelAPITest
{
    
    class Program
    {

        static void Main(string[] args)
        {
            /*string contents = File.ReadAllText(@"TestFiles\features.yml");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  
                .Build();

            var p = deserializer.Deserialize<FeatureSet>(contents);
            foreach(Feature f in p.Features)
            {
                Console.WriteLine($"Feature: {f.Name}, Elements:");
                foreach(String e in f.Elements)
                {
                    Console.WriteLine(e);
                }
            }*/


            /*if (args[0] != "diff" && args[0] != "all" && args[0] != "features")
            {
                Console.WriteLine("Usage: \nall <File.oml> <outputFile.oml> \nOR \ndiff <oldFile.oml> <newFile.oml> <outputFile.oml> \nOR \nfeatures <configFile.yml> <moduleFile.oml> <outputFile.oml>");
                return;
            }
            if (args[0] == "diff" & args.Length != 4)
            {
                Console.WriteLine("Usage: diff <oldFile.oml> <newFile.oml> <outputFile.oml>");
                return;
            }
            if (args[0] == "all" & args.Length != 3)
            {
                Console.WriteLine("Usage: all <File.oml> <outputFile.oml>");
                return;
            }*/

            if (args[0] != "features" && args.Length != 4)
            {
                Console.WriteLine("Usage: features <configFile.yml> <moduleFile.oml> <outputFile.oml>");
                return;
            }

            /*if (args[0] == "diff" & args.Length == 4)
            {
                RunDiff.RunForDiffElements(args);
            }

            if (args[0] == "all" & args.Length == 3)
            {
                RunAll.RunForAllElements(args);
            }*/

             if (args[0] == "features" & args.Length == 4)
            {
                RunContext.RunForSpecificFeatures(args);
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
