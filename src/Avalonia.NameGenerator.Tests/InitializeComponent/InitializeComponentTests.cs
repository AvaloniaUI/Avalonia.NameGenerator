using System.Threading.Tasks;
using Avalonia.NameGenerator.Compiler;
using Avalonia.NameGenerator.Generator;
using Avalonia.NameGenerator.Tests.InitializeComponent.GeneratedInitializeComponent;
using Avalonia.NameGenerator.Tests.OnlyProperties.GeneratedCode;
using Avalonia.NameGenerator.Tests.Views;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Avalonia.NameGenerator.Tests.InitializeComponent
{
    public class InitializeComponentTests
    {
        [Theory]
        [InlineData(InitializeComponentCode.NamedControl, View.NamedControl, true)]
        [InlineData(InitializeComponentCode.NamedControls, View.NamedControls, true)]
        [InlineData(InitializeComponentCode.XNamedControl, View.XNamedControl, true)]
        [InlineData(InitializeComponentCode.XNamedControls, View.XNamedControls, true)]
        [InlineData(InitializeComponentCode.NoNamedControls, View.NoNamedControls, true)]
        [InlineData(InitializeComponentCode.CustomControls, View.CustomControls, true)]
        [InlineData(InitializeComponentCode.DataTemplates, View.DataTemplates, true)]
        [InlineData(InitializeComponentCode.SignUpView, View.SignUpView, true)]
        [InlineData(InitializeComponentCode.AttachedProps, View.AttachedProps, true)]
        [InlineData(InitializeComponentCode.FieldModifier, View.FieldModifier, true)]
        [InlineData(InitializeComponentCode.ControlWithoutWindow, View.ControlWithoutWindow, true)]
        public async Task Should_Generate_FindControl_Refs_From_Avalonia_Markup_File(
            string expectation,
            string markup,
            bool devToolsMode)
        {
            var excluded = devToolsMode ? null : "Avalonia.Diagnostics";
            var compilation =
                View.CreateAvaloniaCompilation(excluded)
                    .WithCustomTextBox();

            var types = new RoslynTypeSystem(compilation);
            var classResolver = new XamlXViewResolver(
                types,
                MiniCompiler.CreateDefault(
                    new RoslynTypeSystem(compilation),
                    MiniCompiler.AvaloniaXmlnsDefinitionAttribute));

            var xaml = await View.Load(markup);
            var classInfo = classResolver.ResolveView(xaml);
            var nameResolver = new XamlXNameResolver();
            var names = nameResolver.ResolveNames(classInfo.Xaml);

            var generator = new InitializeComponentCodeGenerator(types);

            var code = generator
                .GenerateCode("SampleView", "Sample.App",  classInfo.ControlType, names)
                .Replace("\r", string.Empty);

            var expected = await InitializeComponentCode.Load(expectation);
            
            
            CSharpSyntaxTree.ParseText(code);
            Assert.Equal(expected.Replace("\r", string.Empty), code);
        }
    }
}