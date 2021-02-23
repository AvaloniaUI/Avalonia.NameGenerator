using System;
using XamlX.Ast;

namespace Avalonia.NameGenerator.Domain
{
    internal interface IViewResolver
    {
        ResolvedView ResolveView(string xaml);
    }

    internal record ResolvedView(string ClassName, ControlType ControlType, string Namespace, XamlDocument Xaml);
}