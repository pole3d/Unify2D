using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Runtime.Loader;
using Unify2D.Scripting;

namespace Unify2D
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new GameEditor()) game.Run();
        }
    }
}