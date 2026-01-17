namespace JetBrains.Annotations {
    using System;
    using System.Diagnostics;

    /// <summary>
    /// This annotation allows enforcing allocation-less usage patterns of delegates for performance-critical APIs.
    /// When this annotation is applied to the parameter of a delegate type,
    /// the IDE checks the input argument of this parameter:
    /// * When a lambda expression or anonymous method is passed as an argument, the IDE verifies that the passed closure
    ///   has no captures of the containing local variables and the compiler is able to cache the delegate instance
    ///   to avoid heap allocations. Otherwise, a warning is produced.
    /// * The IDE warns when the method name or local function name is passed as an argument because this always results
    ///   in heap allocation of the delegate instance.
    /// </summary>
    /// <remarks>
    /// In C# 9.0+ code, the IDE will also suggest annotating the anonymous functions with the <c>static</c> modifier
    /// to make use of the similar analysis provided by the language/compiler.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    public sealed class RequireStaticDelegateAttribute : Attribute {
        public bool IsError { get; set; }
    }
}