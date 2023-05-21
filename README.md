# CodeAnalyzer

[![test](https://github.com/rnowif/CodeAnalyzer/actions/workflows/ci.yml/badge.svg)](https://github.com/rnowif/CodeAnalyzer/actions/workflows/ci.yml)

This library allows you to analyse a code base and extract code quality metrics.

## Initialise and run the analyser

```csharp
const string sourcesDir = @"C:\Path\To\My\Code";

var analyzer = SourceAnalyzer.FromDirectory(sourcesDir);

var report = analyzer.Analyze(AnalysisConfiguration.Default);
```

## Filtering classes, fields and properties

To filter classes, fields or properties, you can provide a custom configuration. The filtered items will not be considered in the analysis.

For instance, to ignore tests:
```csharp
var configuration = AnalysisConfiguration.New()
            .WhereNodes(node => !node.Identifier.EndsWith("Tests") && !node.Identifier.EndsWith("Test"))
            .Build();
```

Otherwise, to ignore fields and properties of your custom type `Infrastructure.MyLogger`:
```csharp
var typesToIgnore = new List<string> { "Infrastructure.MyLogger" };

var configuration = AnalysisConfiguration.New()
            .WhereFields(field => !typesToIgnore.Contains(field.Type.Name))
            .WhereProperties(property => !typesToIgnore.Contains(property.Type.Name))
            .Build();
```

## Metrics

For now, the library provides the following metrics:
* [Dependency Count](#depency-count)
* [Coupling Between Objects (CBO)](#coupling-between-objects)
* [Tight Class Cohesion (TCC)](#class-cohesion)
* [Loose Class Cohesion (LCC)](#class-cohsion)
* [Lack of Cohesion of Methods (LCOM4)](#lack-of-cohesion-of-methods)

### Dependency Count

The dependencies of a class is all the external types that the class relies on. We exclude types in the global namespace and in the `System` namespace as they offer little relevance to our analysis.

This metric is accessible via the `DependencyCount` property of `ClassAnalysisReport`.

```csharp
var worstDependencyOffenders = report.ClassesReports
    .Select(r => r.Value)
    .OrderByDescending(r => r.DependencyCount)
    .Take(5);

Console.WriteLine("Top 5 worst Dependency Count offenders");
foreach (var offender in worstDependencyOffenders)
{
    Console.WriteLine($"\t- {offender.Identifier} has {offender.DependencyCount} dependencies");
}
```

For information, we take generic types into consideration e.g. `IEnumerable<MyClas>` will return `MyClass` as a dependency.

### Coupling Between Objects

The Coupling between objects (CBO) is the average number of dependencies per class.
According to the literature, there is a direct relationship between the CBO and the number of defects in a codebase.

This metric is accessible via the `CouplingBetweenObjects` property of `AnalysisReport`.

### Class Cohesion

These metrics use the concept of direct and indirect method connection:
* All methods accessing the same variables (even through other methods) are directly connected to each other
* If 2 methods share a direct connection, they are indirectly connected

The Tight Class Cohesion (TCC) is equal to the number of direct connections divided by the number of possible connections between methods.
The Loose Class Cohesion (LCC) is equal to the number of direct *and* indirect connections  divided by the number of possible connections between methods.

* TCC < 0.5 and LCC < 0.5 are considered non-cohesive classes.
* LCC = 0.8 is considered "quite cohesive".
* TCC = LCC = 1 is a maximally cohesive class: all methods are connected.

This metric is accessible via the `TightClassCohesion` and `LooseClassCohesion` properties of `ClassAnalysisReport`.

```csharp
 var worstTccOffenders = report.ClassesReports
    .Select(r => r.Value)
    .OrderBy(r => r.TightClassCohesion)
    .Take(5);

Console.WriteLine("Top 5 worst Tight Class Cohesion (TCC) offenders");
foreach (var offender in worstTccOffenders)
{
    Console.WriteLine($"\t- {offender.Identifier} has a TCC of {offender.TightClassCohesion}");
}
```

For information, we only take visible methods into account as introducing arbitrary private methods should not change these metrics.

### Lack of Cohesion of Methods

The Lack of Cohesion of Methods (LCOM4) measures the number of method groups in a class.
Two methods belong to the same group if they are directly connected or one of the methods call the other one.

* LCOM4 = 1 indicates a cohesive class.
* LCOM4 >= 2 indicates a problem. The class should be split into so many smaller classes.
* LCOM4 = 0 happens when there are no methods in a class.

This metric is accessible via the `LackOfCohesionOfMethods` property of `ClassAnalysisReport`. The method groups themselves can be accessed through the `MethodGroups` property.

```csharp
var worstLCom4Offenders = report.ClassesReports
    .Select(r => r.Value)
    .OrderByDescending(r => r.LackOfCohesionOfMethods)
    .Take(5);

Console.WriteLine("Top 5 worst Lack of Cohesion of Methods (LCOM4) offenders");
foreach (var offender in worstLCom4Offenders)
{
    Console.WriteLine($"\t- {offender.Identifier} has a LCOM4 of {offender.LackOfCohesionOfMethods}");
}

foreach (var methodGroup in report.ClassesReports[ClassToAnalyze].MethodGroups)
{
    foreach (var method in methodGroup)
    {
        Console.WriteLine($"\t- {method}");
    }
    Console.WriteLine("\t ---------------------");
}
```

For information, we only take visible methods into account as introducing arbitrary private methods should not change this metric.

## Sources

* https://www.aivosto.com/project/help/pm-oo-cohesion.html
* https://www.researchgate.net/publication/2540411_Thresholds_for_Object-Oriented_Measures
