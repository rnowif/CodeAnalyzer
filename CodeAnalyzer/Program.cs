using System;
using CodeAnalyzer.Tree;

namespace CodeAnalyzer
{
    class Program
    {
        private const string SourcesDir = @"C:\WF\LP\server";

        public static void Main(string[] args)
        {
            var tree = SourceTree.FromSources(SourcesDir);
            Console.WriteLine(tree.ToString());
        }
    }
}
