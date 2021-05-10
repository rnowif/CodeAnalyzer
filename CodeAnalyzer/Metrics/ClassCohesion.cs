using CodeAnalyzer.Analyzer;

namespace CodeAnalyzer.Metrics
{
    public static class ClassCohesion
    {
        // TCC < 0.5 and LCC<0.5 are considered non-cohesive classes.
        // LCC=0.8 is considered "quite cohesive".
        // TCC=LCC=1 is a maximally cohesive class: all methods are connected.

        /// <summary>
        /// NP = maximum number of possible connections = N * (N-1) / 2 where N is the number of methods
        /// NDC = number of direct connections (number of edges in the connection graph)
        /// Tight class cohesion TCC = NDC/NP
        /// </summary>
        public static float ComputeTightClassCohesion(this ClassAnalyzer classAnalyzer)
        {
            var numberOfPublicMethods = classAnalyzer.MethodGraph.CountVisible;
            var numberOfPossibleConnections = numberOfPublicMethods * (numberOfPublicMethods - 1) / 2;

            return classAnalyzer.MethodGraph.CountDirectConnections / (float) numberOfPossibleConnections;
        }

        /// <summary>
        /// NP = maximum number of possible connections = N * (N-1) / 2 where N is the number of methods
        /// NDC = number of direct connections (number of edges in the connection graph)
        /// NID = number of indirect connections
        /// TLoose class cohesion LCC = (NDC+NIC)/NP
        /// </summary>
        public static float ComputeLooseClassCohesion(this ClassAnalyzer classAnalyzer)
        {
            return 0;
        }
    }
}
