using System.Collections.Generic;
using System.Linq;
using Avalonia.NameGenerator.Compiler;
using Avalonia.NameGenerator.Domain;
using XamlX;
using XamlX.Ast;
using XamlX.Parsers;

namespace Avalonia.NameGenerator.Generator
{
    internal class XamlXClassResolver : IClassResolver, IXamlAstVisitor
    {
        private readonly MiniCompiler _compiler;
        private ResolvedClass _resolvedClass;

        public XamlXClassResolver(MiniCompiler compiler) => _compiler = compiler;

        public ResolvedClass ResolveClass(string xaml)
        {
            var parsed = XDocumentXamlParser.Parse(xaml, new Dictionary<string, string>
            {
                {XamlNamespaces.Blend2008, XamlNamespaces.Blend2008}
            });

            _compiler.Transform(parsed);
            parsed.Root.Visit(this);
            parsed.Root.VisitChildren(this);
            return _resolvedClass;
        }

        IXamlAstNode IXamlAstVisitor.Visit(IXamlAstNode node)
        {
            if (node is not XamlAstObjectNode objectNode)
                return node;

            var clrType = objectNode.Type.GetClrType();
            var isAvaloniaControl = clrType
                .Interfaces
                .Any(abstraction => abstraction.IsInterface &&
                                    abstraction.FullName == "Avalonia.Controls.IControl");

            if (!isAvaloniaControl)
                return node;

            foreach (var child in objectNode.Children)
            {
                if (child is XamlAstXmlDirective directive &&
                    directive.Name == "Class" &&
                    directive.Namespace == XamlNamespaces.Xaml2006 &&
                    directive.Values[0] is XamlAstTextNode text)
                {
                    var split = text.Text.Split('.');
                    var nameSpace = string.Join(".", split.Take(split.Length - 1));
                    var className = split.Last();
                    _resolvedClass = new ResolvedClass(className, nameSpace);
                    return node;
                }
            }

            return node;
        }

        void IXamlAstVisitor.Push(IXamlAstNode node) { }

        void IXamlAstVisitor.Pop() { }
    }
}