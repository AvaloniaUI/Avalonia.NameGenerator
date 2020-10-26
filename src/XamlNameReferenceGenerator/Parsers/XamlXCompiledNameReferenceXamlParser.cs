using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using XamlX;
using XamlX.Compiler;
using XamlX.Emit;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace XamlNameReferenceGenerator.Parsers
{
    public class XamlXCompiledNameReferenceXamlParser : INameReferenceXamlParser
    {
        private readonly IAssemblySymbol _assembly;

        public XamlXCompiledNameReferenceXamlParser(IAssemblySymbol assembly) => _assembly = assembly;

        public List<(string TypeName, string Name)> GetNamedControls(string xaml)
        {
            var parsed = XDocumentXamlParser.Parse(xaml, new Dictionary<string, string>
            {
                {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008}
            });
            
            var assembly = new RoslynAssembly(_assembly);
            var typeSystem = new RoslynTypeSystem(assembly);
            var compiler = new MiniCompiler(
                new TransformerConfiguration(typeSystem, assembly,
                    new XamlLanguageTypeMappings(typeSystem)),
                new XamlLanguageEmitMappings<object, IXamlEmitResult>(),
                false);

            compiler.Transform(parsed);
            return new List<(string TypeName, string Name)>
            {
                ("OK", "OK")
            };
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