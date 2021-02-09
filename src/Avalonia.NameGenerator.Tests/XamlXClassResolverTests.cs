using System.Threading.Tasks;
using Avalonia.NameGenerator.Compiler;
using Avalonia.NameGenerator.Resolver;
using Avalonia.NameGenerator.Tests.Views;
using Xunit;

namespace Avalonia.NameGenerator.Tests
{
    public class XamlXClassResolverTests
    {
        [Theory]
        [InlineData("Sample.App", "NamedControl", View.NamedControl)]
        [InlineData("Sample.App", "AttachedProps", View.AttachedProps)]
        [InlineData("Sample.App", "CustomControls", View.CustomControls)]
        [InlineData("Sample.App", "DataTemplates", View.DataTemplates)]
        [InlineData("Sample.App", "FieldModifier", View.FieldModifier)]
        [InlineData("Sample.App", "NamedControls", View.NamedControls)]
        [InlineData("Sample.App", "NoNamedControls", View.NoNamedControls)]
        [InlineData("Sample.App", "SignUpView", View.SignUpView)]
        [InlineData("Sample.App", "xNamedControl", View.XNamedControl)]
        [InlineData("Sample.App", "xNamedControls", View.XNamedControls)]
        public async Task Should_Resolve_Base_Class_From_Xaml_File(string nameSpace, string className, string markup)
        {
            var xaml = await View.Load(markup);
            var compilation = View
                .CreateAvaloniaCompilation()
                .WithCustomTextBox();

            var resolver = new XamlXClassResolver(
                MiniCompiler.CreateDefault(
                    new RoslynTypeSystem(compilation),
                    MiniCompiler.AvaloniaXmlnsDefinitionAttribute));

            var resolvedClass = resolver.ResolveClass(xaml);
            Assert.Equal(className, resolvedClass.ClassName);
            Assert.Equal(nameSpace, resolvedClass.NameSpace);
        }
    }
}