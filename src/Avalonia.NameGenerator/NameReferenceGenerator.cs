using System.Runtime.CompilerServices;
using Avalonia.NameGenerator.Compiler;
using Avalonia.NameGenerator.Domain;
using Avalonia.NameGenerator.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

[assembly: InternalsVisibleTo("Avalonia.NameGenerator.Tests")]

namespace Avalonia.NameGenerator
{
    [Generator]
    public class NameReferenceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = (CSharpCompilation)context.Compilation;
            var types = new RoslynTypeSystem(compilation);
            var compiler = MiniCompiler.CreateDefault(types, MiniCompiler.AvaloniaXmlnsDefinitionAttribute);

            INameGenerator avaloniaNameGenerator =
                new AvaloniaNameGenerator(
                    new XamlXClassResolver(types, compiler, type => ReportInvalidType(context, type)),
                    new XamlXNameResolver(compiler),
                    new FindControlNameGenerator());

            var partials = avaloniaNameGenerator.GenerateNameReferences(context.AdditionalFiles);
            foreach (var partial in partials) context.AddSource(partial.FileName, partial.Content);
        }

        private static void ReportInvalidType(GeneratorExecutionContext context, string typeName)
        {
            var message = $"Avalonia x:Name generator was unable to generate names for type '{typeName}'. " +
                          $"The type '{typeName}' does not exist in the assembly.";
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "AXN0003",
                        message,
                        message,
                        "Usage",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
        }
    }
}
