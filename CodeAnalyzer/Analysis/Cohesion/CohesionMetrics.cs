namespace CodeAnalyzer.Analysis.Cohesion
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
        public static float? ComputeTightClassCohesion(this ClassCohesionAnalyzer analyzer)
        {
            var numberOfPublicMethods = analyzer.CountVisible;
            var numberOfPossibleConnections = numberOfPublicMethods * (numberOfPublicMethods - 1) / 2;

            return numberOfPossibleConnections > 0
                ? analyzer.CountDirectConnections / (float) numberOfPossibleConnections
                : (float?) null;
        }

        /// <summary>
        /// NP = maximum number of possible connections = N * (N-1) / 2 where N is the number of methods
        /// NDC = number of direct connections (number of edges in the connection graph)
        /// NID = number of indirect connections
        /// TLoose class cohesion LCC = (NDC+NIC)/NP
        /// </summary>
        public static float? ComputeLooseClassCohesion(this ClassCohesionAnalyzer analyzer)
        {
            var numberOfPublicMethods = analyzer.CountVisible;
            var numberOfPossibleConnections = numberOfPublicMethods * (numberOfPublicMethods - 1) / 2;

            return numberOfPossibleConnections > 0
                ? (analyzer.CountIndirectConnections + analyzer.CountDirectConnections) / (float) numberOfPossibleConnections
                : (float?) null;
        }

        /// <summary>
        /// LCOM4 measures the number of "connected components" in a class.
        ///
        /// - LCOM4=1 indicates a cohesive class, which is the "good" class.
        /// - LCOM4>=2 indicates a problem. The class should be split into so many smaller classes.
        /// - LCOM4=0 happens when there are no methods in a class.
        /// </summary>
        public static int ComputeLackOfCohesionOfMethod(this ClassCohesionAnalyzer analyzer) => analyzer.GetMethodGroups();
    }
}
