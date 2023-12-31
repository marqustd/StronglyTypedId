using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using StronglyTypedIds;
using Xunit;

namespace StronglyTypedIds.Tests;

public class StronglyTypedRecordSourceGeneratorTests
{
    private const string VectorClassText =
        """
        namespace TestNamespace;

        [StronglyTypedIds.StronglyTypedIds]
        public partial record TestedRecord;
        """;

    private const string ExpectedGeneratedClassText =
        """
        // <auto-generated/>
        namespace TestNamespace;

        partial record TestedRecord
        {
            public string Value { get; }
            
            public TestedRecord(string value)
            {
                Value = value.ToUpperInvariant();
            }
            
            public static implicit operator string(TestedRecord stronglyTyped) => stronglyTyped.ToString();
        
            public static explicit operator TestedRecord(string value) => new(value);
        
            public override string ToString() => Value;
        }
        """;

    [Fact]
    public void GenerateReportMethod()
    {
        // Create an instance of the source generator.
        var generator = new StronglyTypedRecordSourceGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(generator);

        // We need to create a compilation with the required source code.
        var compilation = CSharpCompilation.Create(nameof(StronglyTypedRecordSourceGeneratorTests),
            new[]
            {
                CSharpSyntaxTree.ParseText(VectorClassText)
            },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators and retrieve all results.
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        // All generated files can be found in 'RunResults.GeneratedTrees'.
        var generatedFileSyntax = runResult.GeneratedTrees.Single(t => t.FilePath.EndsWith("TestedRecord.g.cs"));

        // Complex generators should be tested using text comparison.
        Assert.Equal(ExpectedGeneratedClassText,
            generatedFileSyntax.GetText().ToString(),
            ignoreLineEndingDifferences: true);
    }
}