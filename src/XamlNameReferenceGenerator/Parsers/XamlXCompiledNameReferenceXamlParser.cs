using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using XamlX;
using XamlX.Ast;
using XamlX.Compiler;
using XamlX.Emit;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace XamlNameReferenceGenerator.Parsers
{
    public class XamlXCompiledNameReferenceXamlParser : INameReferenceXamlParser
    {
        private readonly CSharpCompilation _compilation;

        public XamlXCompiledNameReferenceXamlParser(CSharpCompilation compilation) => _compilation = compilation;

        public List<(string TypeName, string Name)> GetNamedControls(string xaml)
        {
            var parsed = XDocumentXamlParser.Parse(xaml, new Dictionary<string, string>
            {
                {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008}
            });
            
            var typeSystem = new RoslynTypeSystem(_compilation);
            var compiler = new MiniCompiler(
                new TransformerConfiguration(
                    typeSystem,
                    typeSystem.Assemblies[0],
                    new XamlLanguageTypeMappings(typeSystem)),
                new XamlLanguageEmitMappings<object, IXamlEmitResult>(),
                true);

            compiler.Transform(parsed);
            
            var visitor = new MiniVisitor();
            parsed.Root.Visit(visitor);
            parsed.Root.VisitChildren(visitor);
            return visitor.Controls;
        }

        private class MiniVisitor : IXamlAstVisitor
        {
            public List<(string TypeName, string Name)> Controls { get; } = new List<(string TypeName, string Name)>();

            public IXamlAstNode Visit(IXamlAstNode node)
            {
                if (node is XamlAstObjectNode element && element.Type is XamlAstXmlTypeReference type)
                {
                    Controls.Add((type.Name, $@"{type.XmlNamespace} {node.Line} {node.Position}"));
                }

                return node;
            }

            public void Push(IXamlAstNode node) { }

            public void Pop() { }
        }

        private class MiniCompiler : XamlCompiler<object, IXamlEmitResult>
        {
            public MiniCompiler(
                TransformerConfiguration configuration,
                XamlLanguageEmitMappings<object, IXamlEmitResult> emitMappings,
                bool fillWithDefaults)
                : base(configuration, emitMappings, fillWithDefaults)
            {
            }

            protected override XamlEmitContext<object, IXamlEmitResult> InitCodeGen(
                IFileSource file,
                Func<string, IXamlType, IXamlTypeBuilder<object>> createSubType,
                object codeGen, XamlRuntimeContext<object, IXamlEmitResult> context,
                bool needContextLocal) =>
                throw new NotSupportedException();
        }
    }
}