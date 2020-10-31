using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using XamlNameReferenceGenerator.Infrastructure;
using Xunit;

namespace XamlNameReferenceGenerator.Tests
{
    public class NameResolverTests
    {
        private const string NamedControl = "XamlNameReferenceGenerator.Tests.Samples.NamedControl.xml";
        private const string NamedControls = "XamlNameReferenceGenerator.Tests.Samples.NamedControls.xml";
        private const string XNamedControl = "XamlNameReferenceGenerator.Tests.Samples.xNamedControl.xml";
        private const string XNamedControls = "XamlNameReferenceGenerator.Tests.Samples.xNamedControls.xml";
        private const string NoNamedControls = "XamlNameReferenceGenerator.Tests.Samples.NoNamedControls.xml";
        private const string CustomControls = "XamlNameReferenceGenerator.Tests.Samples.CustomControls.xml";
        private const string DataTemplates = "XamlNameReferenceGenerator.Tests.Samples.DataTemplates.xml";
        
        [Theory]
        [InlineData(NamedControl)]
        [InlineData(XNamedControl)]
        public async Task Should_Resolve_Types_From_Avalonia_Markup_File_With_Named_Control(string resource)
        {
            var xaml = await LoadEmbeddedResource(resource);
            var compilation = CreateAvaloniaCompilation();
            var resolver = new NameResolver(compilation);
            var controls = resolver.ResolveNames(xaml);

            Assert.NotEmpty(controls);
            Assert.Equal(1, controls.Count);
            Assert.Equal("UserNameTextBox", controls[0].Name);
            Assert.Equal(typeof(TextBox).FullName, controls[0].TypeName);
        }

        [Theory]
        [InlineData(NamedControls)]
        [InlineData(XNamedControls)]
        public async Task Should_Resolve_Types_From_Avalonia_Markup_File_With_Named_Controls(string resource)
        {
            var xaml = await LoadEmbeddedResource(resource);
            var compilation = CreateAvaloniaCompilation();
            var resolver = new NameResolver(compilation);
            var controls = resolver.ResolveNames(xaml);

            Assert.NotEmpty(controls);
            Assert.Equal(3, controls.Count);
            Assert.Equal("UserNameTextBox", controls[0].Name);
            Assert.Equal("PasswordTextBox", controls[1].Name);
            Assert.Equal("SignUpButton", controls[2].Name);
            Assert.Equal(typeof(TextBox).FullName, controls[0].TypeName);
            Assert.Equal(typeof(TextBox).FullName, controls[1].TypeName);
            Assert.Equal(typeof(Button).FullName, controls[2].TypeName);
        }

        [Fact]
        public async Task Should_Resolve_Types_From_Avalonia_Markup_File_With_Custom_Controls()
        {
            var xaml = await LoadEmbeddedResource(CustomControls);
            var compilation = CreateAvaloniaCompilation();
            var resolver = new NameResolver(compilation);
            var controls = resolver.ResolveNames(xaml);

            Assert.NotEmpty(controls);
            Assert.Equal(2, controls.Count);
            Assert.Equal("ClrNamespaceRoutedViewHost", controls[0].Name);
            Assert.Equal("UriRoutedViewHost", controls[1].Name);
            Assert.Equal(typeof(RoutedViewHost).FullName, controls[0].TypeName);
            Assert.Equal(typeof(RoutedViewHost).FullName, controls[1].TypeName);
        }
        
        [Fact]
        public async Task Should_Not_Resolve_Named_Controls_From_Avalonia_Markup_File_Without_Named_Controls()
        {
            var xaml = await LoadEmbeddedResource(NoNamedControls);
            var compilation = CreateAvaloniaCompilation();
            var resolver = new NameResolver(compilation);
            var controls = resolver.ResolveNames(xaml);

            Assert.Empty(controls);
        }

        [Fact]
        public async Task Should_Not_Resolve_Elements_From_DataTemplates()
        {
            var xaml = await LoadEmbeddedResource(DataTemplates);
            var compilation = CreateAvaloniaCompilation();
            var resolver = new NameResolver(compilation);
            var controls = resolver.ResolveNames(xaml);
            
            Assert.NotEmpty(controls);
            Assert.Equal(2, controls.Count);
            Assert.Equal("UserNameTextBox", controls[0].Name);
            Assert.Equal("NamedListBox", controls[1].Name);
            Assert.Equal(typeof(TextBox).FullName, controls[0].TypeName);
            Assert.Equal(typeof(ListBox).FullName, controls[1].TypeName);
        }
        
        private static CSharpCompilation CreateAvaloniaCompilation(string name = "AvaloniaCompilation2")
        {
            var compilation = CSharpCompilation
                .Create(name, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(ITypeDescriptorContext).Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(ISupportInitialize).Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(TypeConverterAttribute).Assembly.Location));
            
            var avaloniaAssemblyLocation = typeof(TextBlock).Assembly.Location;
            var avaloniaAssemblyDirectory = Path.GetDirectoryName(avaloniaAssemblyLocation);
            var avaloniaAssemblyReferences = Directory
                .EnumerateFiles(avaloniaAssemblyDirectory!)
                .Where(file => file.EndsWith(".dll") && file.Contains("Avalonia"))
                .Select(file => MetadataReference.CreateFromFile(file))
                .ToList();

            return compilation.AddReferences(avaloniaAssemblyReferences);
        }

        private static async Task<string> LoadEmbeddedResource(string resourceName)
        {
            var assembly = typeof(NameResolverTests).Assembly;
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            return await reader.ReadToEndAsync();
        }
    }
}