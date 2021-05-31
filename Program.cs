using System;
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

namespace ModelAPITest
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args[0] != "diff" && args[0] != "all")
            {
                Console.WriteLine("Usage: \nall <File.oml> <outputFile.oml> \nOR \ndiff <oldFile.oml> <newFile.oml> <outputFile.oml>");
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
            }

            if (args[0] == "diff" & args.Length == 4)
            {
                RunDifferential.RunForDiffElements(args);
            }

            if (args[0] == "all" & args.Length == 3)
            {
                RunAll.RunForAllElements(args);

                
            }


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
