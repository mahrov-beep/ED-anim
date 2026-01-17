namespace Game.ServerRunner.Core;

using System.CommandLine;
using System.CommandLine.Invocation;
using JetBrains.Annotations;

public static class CommandLineOptionExtensions {
    [PublicAPI]
    [NotNull]
    public static T GetValueFrom<T>(this Option<T> option, InvocationContext invocationContext) {
        return invocationContext.ParseResult.GetValueForOption(option);
    }
}