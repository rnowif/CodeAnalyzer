using System.Collections.Generic;
using System.Linq;

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
        public int LackOfCohesionOfMethods => MethodGroups.Count();
        public float TightClassCohesion { get; }
        public float LooseClassCohesion { get; }
        public IEnumerable<IEnumerable<string>> MethodGroups { get; }

        public ClassAnalysisReport(string identifier, int dependencyCount, float? tightClassCohesion, float? looseClassCohesion, IEnumerable<IEnumerable<string>> methodGroups)
        {
            Identifier = identifier;
            TightClassCohesion = tightClassCohesion ?? 1;
            LooseClassCohesion = looseClassCohesion ?? 1;
            DependencyCount = dependencyCount;
            MethodGroups = methodGroups;
        }
    }
}
