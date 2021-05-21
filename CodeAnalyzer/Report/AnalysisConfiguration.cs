using System;
using CodeAnalyzer.Analysis.Coupling;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Report
{
    public class AnalysisConfiguration
    {
        public static readonly AnalysisConfiguration Default = New().Build();

        public Func<DependencyNode, bool> SelectNode { get; }
        public Func<IFieldSymbol, bool> SelectField { get; }
        public Func<IPropertySymbol, bool> SelectProperty { get; }

        private AnalysisConfiguration(Func<DependencyNode, bool> selectNode, Func<IFieldSymbol, bool> selectField, Func<IPropertySymbol, bool> selectProperty)
        {
            SelectNode = selectNode;
            SelectField = selectField;
            SelectProperty = selectProperty;
        }

        public static Builder New() => new Builder();

        public class Builder
        {
            private Func<DependencyNode, bool> _selectNode = node => true;
            private Func<IFieldSymbol, bool> _selectField = symbol => true;
            private Func<IPropertySymbol, bool> _selectProperty = symbol => true;

            public Builder WhereNodes(Func<DependencyNode, bool> selectNode)
            {
                _selectNode = selectNode;

                return this;
            }

            public Builder WhereFields(Func<IFieldSymbol, bool> selectField)
            {
                _selectField = selectField;

                return this;
            }

            public Builder WhereProperties(Func<IPropertySymbol, bool> selectProperty)
            {
                _selectProperty = selectProperty;

                return this;
            }

            public AnalysisConfiguration Build() => new AnalysisConfiguration(_selectNode, _selectField, _selectProperty);
        }
    }
}
