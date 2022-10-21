using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Unify2D
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //Console.WriteLine("Creating new AppDomain.");
            //AppDomain domain = AppDomain.CreateDomain("MyDomain");

            //Console.WriteLine("Host domain: " + AppDomain.CurrentDomain.FriendlyName);
            //Console.WriteLine("child domain: " + domain.FriendlyName);
            //try
            //{
            //    AppDomain.Unload(domain);
            //    Console.WriteLine();
            //    Console.WriteLine("Host domain: " + AppDomain.CurrentDomain.FriendlyName);
            //    // The following statement creates an exception because the domain no longer exists.
            //    Console.WriteLine("child domain: " + domain.FriendlyName);
            //}
            //catch (AppDomainUnloadedException e)
            //{
            //    Console.WriteLine(e.GetType().FullName);
            //    Console.WriteLine("The appdomain MyDomain does not exist.");
            //}

//            var bar1Code = @"
//public class Bar1 
//{
//    public Bar1(int value)
//    {
//        NewProperty = value;
//    }
//    public int NewProperty {get; set; }
//}
//";

//#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
//            var compilerResults = new CSharpCodeProvider()
//    .CompileAssemblyFromSource(
//        new CompilerParameters
//        {
//            GenerateInMemory = true,
//            ReferencedAssemblies =
//            {
//                "System.dll"//,
//                //Assembly.GetExecutingAssembly().Location
//            }
//        },
//        bar1Code);
//            var bar1Type = compilerResults.CompiledAssembly.GetType("Bar1");
//            var bar2Type = compilerResults.CompiledAssembly.GetType("Bar2"); // By analogy



//            dynamic v = Activator.CreateInstance(bar1Type, 56);

//            Console.WriteLine(v.NewProperty);
//#pragma warning restore CA1416 // Valider la compatibilité de la plateforme




            using (var game = new GameEditor()) game.Run();
        }
    }
}