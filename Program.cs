using System;
using System.IO;
using OutSystems.Model.UI.Web;

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

            var eSpacePath = Console.ReadLine();

            //var eSpacePath = args[0];
            if (!File.Exists(eSpacePath))
            {
                Console.WriteLine($"File {eSpacePath} not found");
                return;
            }

            var modelServices = OutSystems.ModelAPILoader.Loader.ModelServicesInstance;

            var module = modelServices.LoadESpace(eSpacePath);

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

        
    }
}
