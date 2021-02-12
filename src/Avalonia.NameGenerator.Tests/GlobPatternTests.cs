using Avalonia.NameGenerator.Generator;
using Xunit;

namespace Avalonia.NameGenerator.Tests
{
    public class GlobPatternTests
    {
        [Theory]
        [InlineData("*", "anything", true)]
        [InlineData("", "anything", false)]
        [InlineData("Views/*", "Views/SignUpView.xaml", true)]
        [InlineData("Views/*", "Extensions/SignUpView.xaml", false)]
        [InlineData("*SignUpView*", "Extensions/SignUpView.xaml", true)]
        [InlineData("*SignUpView.paml", "Extensions/SignUpView.xaml", false)]
        [InlineData("*.xaml", "Extensions/SignUpView.xaml", true)]
        public void Should_Match_Glob_Expressions(string pattern, string value, bool matches)
        {
            Assert.Equal(matches, new GlobPattern(pattern).Matches(value));
        }
    }
}