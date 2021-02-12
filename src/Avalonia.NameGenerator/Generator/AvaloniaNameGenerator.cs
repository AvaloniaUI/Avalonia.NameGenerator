using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.NameGenerator.Domain;
using Microsoft.CodeAnalysis;

namespace Avalonia.NameGenerator.Generator
{
    internal class AvaloniaNameGenerator : INameGenerator
    {
        private readonly GlobPattern _pathPattern;
        private readonly IViewResolver _classes;
        private readonly INameResolver _names;
        private readonly ICodeGenerator _code;

        public AvaloniaNameGenerator(
            GlobPattern pathPattern,
            IViewResolver classes,
            INameResolver names,
            ICodeGenerator code)
        {
            _pathPattern = pathPattern;
            _classes = classes;
            _names = names;
            _code = code;
        }

        public IReadOnlyList<GeneratedPartialClass> GenerateNameReferences(IEnumerable<AdditionalText> additionalFiles)
        {
            var resolveViewsQuery =
                from file in additionalFiles
                where _pathPattern.Matches(file.Path)
                where file.Path.EndsWith(".xaml") ||
                      file.Path.EndsWith(".paml") ||
                      file.Path.EndsWith(".axaml")
                let xaml = file.GetText()!.ToString()
                let type = _classes.ResolveView(xaml)
                where type != null
                select type;

            var query =
                from view in resolveViewsQuery
                let names = _names.ResolveNames(view.Xaml)
                let code = _code.GenerateCode(view.ClassName, view.NameSpace, names)
                let fileName = $"{view.ClassName}.g.cs"
                select new GeneratedPartialClass(fileName, code);

            return query.ToList();
        }
    }
}