using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using XamlX;
using XamlX.Ast;
using XamlX.Compiler;
using XamlX.Emit;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.Transform.Transformers;
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
                    new XamlLanguageTypeMappings(typeSystem)
                    {
                        XmlnsAttributes =
                        {
                            typeSystem.GetType("Avalonia.Metadata.XmlnsDefinitionAttribute")
                        }
                    })
                );

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
                if (node is XamlAstConstructableObjectNode constructableObjectNode)
                {
                    foreach (var child in constructableObjectNode.Children)
                    {
                        var nameValue = ResolveNameDirectiveOrDefault(child);
                        if (nameValue == null) continue;

                        var clrType = constructableObjectNode.Type.GetClrType().GetFullName();
                        var typeNamePair = (clrType, nameValue);
                        if (!Controls.Contains(typeNamePair))
                            Controls.Add(typeNamePair);
                    }
                }
                
                return node;
            }

            public void Push(IXamlAstNode node) { }

            public void Pop() { }
            
            private static string ResolveNameDirectiveOrDefault(IXamlAstNode node) =>
                node switch
                {
                    XamlAstXamlPropertyValueNode propertyValueNode when
                        propertyValueNode.Property is XamlAstNamePropertyReference reference &&
                        reference.Name == "Name" &&
                        propertyValueNode.Values.Count > 0 &&
                        propertyValueNode.Values[0] is XamlAstTextNode nameNode => nameNode.Text,

                    XamlAstXmlDirective xmlDirective when
                        xmlDirective.Name == "Name" &&
                        xmlDirective.Values.Count > 0 &&
                        xmlDirective.Values[0] is XamlAstTextNode xNameNode => xNameNode.Text,

                    _ => null
                };
        }

        private class MiniCompiler : XamlCompiler<object, IXamlEmitResult>
        {
            public MiniCompiler(TransformerConfiguration configuration)
                : base(configuration, new XamlLanguageEmitMappings<object, IXamlEmitResult>(), false)
            {
                Transformers.Add(new KnownDirectivesTransformer());
                Transformers.Add(new XamlIntrinsicsTransformer());
                Transformers.Add(new XArgumentsTransformer());
                Transformers.Add(new TypeReferenceResolver());
                Transformers.Add(new MarkupExtensionTransformer());
                Transformers.Add(new PropertyReferenceResolver());
                Transformers.Add(new ResolvePropertyValueAddersTransformer());
                Transformers.Add(new ConstructableObjectTransformer());
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