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
            var compiler =
                MiniCompiler.CreateDefault(
                    new RoslynTypeSystem(compilation),
                    MiniCompiler.AvaloniaXmlnsDefinitionAttribute);

            INameGenerator avaloniaNameGenerator =
                new AvaloniaNameGenerator(
                    new XamlXClassResolver(compiler),
                    new XamlXNameResolver(compiler),
                    new FindControlNameGenerator());

            var partials = avaloniaNameGenerator.GenerateNameReferences(context.AdditionalFiles);
            foreach (var partial in partials) context.AddSource(partial.FileName, partial.Content);
        }
    }
}
