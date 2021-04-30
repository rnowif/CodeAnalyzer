using System;
using CodeAnalyzer.Metrics;
using CodeAnalyzer.Tree;

namespace CodeAnalyzer
{
    class Program
    {
        private const string SourcesDir = @"C:\WF\LP\server";

        public static void Main(string[] args)
        {
            var tree = SourceTree.FromDirectory(SourcesDir);
            Console.WriteLine($"Analysed {tree.Classes.Count} classes.");
            Console.WriteLine($"Coupling Between Objects (CBO): {tree.ComputeCouplingBetweenObjects()}");
        }
    }
}
