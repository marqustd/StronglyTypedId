using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace StronglyTypedIds;

[Generator]
public class StronglyTypedRecordSourceGenerator : IIncrementalGenerator
{
    private static readonly string Namespace = typeof(StronglyTypedRecordSourceGenerator).Namespace!;
    private const string AttributeName = nameof(StronglyTypedIdAttribute);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RegisterAttribute(context);
        RegisterValidator(context);
        RegisterStringTransformation(context);

        // Filter classes annotated with the [StronglyTypedIds] attribute. Only filtered Syntax Nodes can trigger code generation.
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is RecordDeclarationSyntax,
                (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.reportAttributeFound)
            .Select((t, _) => t.recordSyntax);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
    }

    private static void RegisterAttribute(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{AttributeName}.g.cs",
            SourceText.From(EmbeddedSources.StronglyTypedIdAttributeSource, Encoding.UTF8)));
    }
    
    private static void RegisterValidator(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{nameof(IValidator)}.g.cs",
            SourceText.From(EmbeddedSources.IValidatorSource, Encoding.UTF8)));
    }
    
    private static void RegisterStringTransformation(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{nameof(StringTransformation)}.g.cs",
            SourceText.From(EmbeddedSources.StringTransformationSource, Encoding.UTF8)));
    }

    /// <summary>
    /// Checks whether the Node is annotated with the [StronglyTypedIds] attribute and maps syntax context to the specific node type (ClassDeclarationSyntax).
    /// </summary>
    /// <param name="context">Syntax context, based on CreateSyntaxProvider predicate</param>
    /// <returns>The specific cast and whether the attribute was found.</returns>
    private static (RecordDeclarationSyntax recordSyntax, bool reportAttributeFound) GetClassDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var recordDeclarationSyntax = (RecordDeclarationSyntax)context.Node;

        // Go through all attributes of the class.
        foreach (AttributeListSyntax attributeListSyntax in recordDeclarationSyntax.AttributeLists)
        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue; // if we can't get the symbol, ignore it

            string attributeName = attributeSymbol.ContainingType.ToDisplayString();

            // Check the full name of the [StronglyTypedIds] attribute.
            if (attributeName == $"{Namespace}.{AttributeName}")
                return (recordDeclarationSyntax, true);
        }

        return (recordDeclarationSyntax, false);
    }

    /// <summary>
    /// Generate code action.
    /// It will be executed on specific nodes (ClassDeclarationSyntax annotated with the [StronglyTypedIds] attribute) changed by the user.
    /// </summary>
    /// <param name="context">Source generation context used to add source files.</param>
    /// <param name="compilation">Compilation used to provide access to the Semantic Model.</param>
    /// <param name="classDeclarations">Nodes annotated with the [StronglyTypedIds] attribute that trigger the generate action.</param>
    private void GenerateCode(SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<RecordDeclarationSyntax> classDeclarations)
    {
        // Go through all filtered class declarations.
        foreach (var recordDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            var semanticModel = compilation.GetSemanticModel(recordDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(recordDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // 'Identifier' means the token of the node. Get class name from the syntax node.
            var className = recordDeclarationSyntax.Identifier.Text;
            
            var checkValidator = classSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == $"{Namespace}.{AttributeName}")?
                .ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;

            // Build up the source code
            var code =
                $$"""
                  // <auto-generated/>
                  namespace {{namespaceName}};

                  partial record {{className}}
                  {
                      public string Value { get; }
                      
                      public {{className}}(string value)
                      {
                        {{Validation(checkValidator)}}
                      
                          Value = value.ToUpperInvariant();
                      }
                      
                      public static implicit operator string({{className}} stronglyTyped) => stronglyTyped.ToString();
                  
                      public static explicit operator {{className}}(string value) => new(value);
                  
                      public override string ToString() => Value;
                  }
                  """;

            // Add the source code to the compilation.
            context.AddSource($"{className}.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }

    private string Validation(INamedTypeSymbol? checkValidator)
    {
        return string.Empty;
    }
}