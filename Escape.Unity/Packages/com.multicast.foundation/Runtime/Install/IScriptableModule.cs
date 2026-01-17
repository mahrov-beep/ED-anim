namespace Multicast.Install {
    using Cysharp.Threading.Tasks;

    public interface IScriptableModule {
        string name { get; }

        bool IsPlatformSupported(string platform);

        void Setup(ScriptableModule.ModuleSetup module);
        
        UniTask Install(ScriptableModule.Resolver resolver);
        
        void PreInstall();
        void PostInstall();
    }

    public interface ICompletableModule {
        UniTask WaitForCompletionAsync();
    }

    public interface IScriptableModuleWithPriority {
        int Priority { get; }
    }

    public interface ISubModuleProvider : IScriptableModule {
        IScriptableModule[] BuildSubModules();
    }

    public interface INonLoggedScriptableModule : IScriptableModule {
    }
}