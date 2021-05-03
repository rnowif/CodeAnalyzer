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
            // See: https://www.researchgate.net/publication/2540411_Thresholds_for_Object-Oriented_Measures for thresholds
            var tree = SourceTree.FromDirectory(SourcesDir);
            Console.WriteLine($"Analysed {tree.Classes.Count} classes.");
            Console.WriteLine($"Coupling Between Objects (CBO): {tree.ComputeCouplingBetweenObjects()}");
        }
    }
}
