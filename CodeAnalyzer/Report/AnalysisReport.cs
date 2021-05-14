using System.Collections.Generic;

namespace CodeAnalyzer.Report
{
    public class AnalysisReport
    {
        public float CouplingBetweenObjects { get; }
        public IDictionary<string, ClassAnalysisReport> ClassesReports { get; }

        public AnalysisReport(float couplingBetweenObjects, IDictionary<string, ClassAnalysisReport> classesReports)
        {
            CouplingBetweenObjects = couplingBetweenObjects;
            ClassesReports = classesReports;
        }
    }

    public class ClassAnalysisReport
    {
        public string Identifier { get; }
        public int DependencyCount { get; }
        public float TightClassCohesion { get; }
        public float LooseClassCohesion { get; }

        public ClassAnalysisReport(string identifier, int dependencyCount, float? tightClassCohesion, float? looseClassCohesion)
        {
            Identifier = identifier;
            TightClassCohesion = tightClassCohesion ?? 1;
            LooseClassCohesion = looseClassCohesion ?? 1;
            DependencyCount = dependencyCount;
        }
    }
}
