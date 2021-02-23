using System.Collections.Generic;

namespace Avalonia.NameGenerator.Domain
{
    internal interface ICodeGenerator
    {
        string GenerateCode(string className, string nameSpace, ControlType type ,IEnumerable<ResolvedName> names);
    }
}