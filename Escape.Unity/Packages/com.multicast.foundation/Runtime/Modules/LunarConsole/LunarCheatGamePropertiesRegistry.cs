namespace Multicast.Modules.LunarConsole {
    using System;
    using Cheats;
    using GameProperties;
    using JetBrains.Annotations;
#if LUNAR_CONSOLE
    using LunarConsolePlugin;
#endif
    using Scellecs.Morpeh;
    using UniMob;
    using UnityEngine;

    internal class LunarCheatGamePropertiesRegistry : ICheatGamePropertiesRegistry {
        private readonly Lifetime            lifetime;
        private readonly World               world;
        private readonly GamePropertiesModel properties;

        public LunarCheatGamePropertiesRegistry(Lifetime lifetime, World world, GamePropertiesModel properties) {
            this.lifetime   = lifetime;
            this.world      = world;
            this.properties = properties;
        }

        [PublicAPI]
        public void Register(BoolGamePropertyName name) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            var cVar          = new CVar(name.Name, false, CFlags.NoArchive);
            var ignoreChanged = false;

            void SetValueWithoutNotify(bool value) {
                var prevValue = cVar.BoolValue;

                ignoreChanged  = true;
                cVar.BoolValue = value;
                ignoreChanged  = false;

                if (prevValue != value) {
                    RefreshVariableInConsole(cVar);
                }
            }

            AddDelegate(this.lifetime, cVar, _ => {
                if (ignoreChanged) {
                    return;
                }

                var newValue = cVar.BoolValue;
                App.Execute(new SetBoolGamePropertyCommand(name, newValue));
                SetValueWithoutNotify(this.properties.Get(name));
            });
            Atom.Reaction(this.lifetime, () => this.properties.Get(name), SetValueWithoutNotify);
            LunarConsole.instance.registry.Register(cVar);
#else
            Debug.LogError("Failed to register cheat property: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }

        [PublicAPI]
        public void Register(IntGamePropertyName name) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            var cVar          = new CVar(name.Name, 0, CFlags.NoArchive);
            var ignoreChanged = false;

            void SetValueWithoutNotify(int value) {
                var prevValue = cVar.IntValue;

                ignoreChanged = true;
                cVar.IntValue = value;
                ignoreChanged = false;

                if (prevValue != value) {
                    RefreshVariableInConsole(cVar);
                }
            }

            AddDelegate(this.lifetime, cVar, _ => {
                if (ignoreChanged) {
                    return;
                }

                var newValue = cVar.IntValue;
                App.Execute(new SetIntGamePropertyCommand(name, newValue));
                SetValueWithoutNotify(this.properties.Get(name));
            });
            Atom.Reaction(this.lifetime, () => this.properties.Get(name), SetValueWithoutNotify);
            LunarConsole.instance.registry.Register(cVar);
#else
            Debug.LogError("Failed to register cheat property: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }

        [PublicAPI]
        public void Register(FloatGamePropertyName name) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            var cVar          = new CVar(name.Name, 0.0f, CFlags.NoArchive);
            var ignoreChanged = false;

            void SetValueWithoutNotify(float value) {
                var prevValue = cVar.FloatValue;

                ignoreChanged   = true;
                cVar.FloatValue = value;
                ignoreChanged   = false;

                if (!Mathf.Approximately(prevValue, value)) {
                    RefreshVariableInConsole(cVar);
                }
            }

            AddDelegate(this.lifetime, cVar, _ => {
                if (ignoreChanged) {
                    return;
                }

                var newValue = cVar.FloatValue;
                App.Execute(new SetFloatGamePropertyCommand(name, newValue));
                SetValueWithoutNotify(this.properties.Get(name));
            });
            Atom.Reaction(this.lifetime, () => this.properties.Get(name), SetValueWithoutNotify);
            LunarConsole.instance.registry.Register(cVar);
#else
            Debug.LogError("Failed to register cheat property: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }

        public void Register(string name, Func<bool> getter, Action<bool> setter = null) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            var cVar          = new CVar(name, false, CFlags.NoArchive);
            var ignoreChanged = false;

            void SetValueWithoutNotify(bool value) {
                var prevValue = cVar.BoolValue;

                ignoreChanged  = true;
                cVar.BoolValue = value;
                ignoreChanged  = false;

                if (prevValue != value) {
                    RefreshVariableInConsole(cVar);
                }
            }

            AddDelegate(this.lifetime, cVar, _ => {
                if (ignoreChanged) {
                    return;
                }

                var newValue = cVar.BoolValue;
                setter?.Invoke(newValue);
                SetValueWithoutNotify(getter.Invoke());
            });
            Atom.Reaction(this.lifetime, () => getter.Invoke(), SetValueWithoutNotify);
            LunarConsole.instance.registry.Register(cVar);
#else
            Debug.LogError("Failed to register cheat property: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }

        public void Register(string name, Func<int> getter, Action<int> setter = null) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            var cVar          = new CVar(name, 0, CFlags.NoArchive);
            var ignoreChanged = false;

            void SetValueWithoutNotify(int value) {
                var prevValue = cVar.IntValue;

                ignoreChanged = true;
                cVar.IntValue = value;
                ignoreChanged = false;

                if (prevValue != value) {
                    RefreshVariableInConsole(cVar);
                }
            }

            AddDelegate(this.lifetime, cVar, _ => {
                if (ignoreChanged) {
                    return;
                }

                var newValue = cVar.IntValue;
                setter?.Invoke(newValue);
                SetValueWithoutNotify(getter.Invoke());
            });
            Atom.Reaction(this.lifetime, () => getter.Invoke(), SetValueWithoutNotify);
            LunarConsole.instance.registry.Register(cVar);
#else
            Debug.LogError("Failed to register cheat property: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }

        public void Register(string name, Func<float> getter, Action<float> setter = null) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            var cVar          = new CVar(name, 0.0f, CFlags.NoArchive);
            var ignoreChanged = false;

            void SetValueWithoutNotify(float value) {
                var prevValue = cVar.FloatValue;

                ignoreChanged   = true;
                cVar.FloatValue = value;
                ignoreChanged   = false;

                if (!Mathf.Approximately(prevValue, value)) {
                    RefreshVariableInConsole(cVar);
                }
            }

            AddDelegate(this.lifetime, cVar, _ => {
                if (ignoreChanged) {
                    return;
                }

                var newValue = cVar.FloatValue;
                setter?.Invoke(newValue);
                SetValueWithoutNotify(getter.Invoke());
            });
            Atom.Reaction(this.lifetime, () => getter.Invoke(), SetValueWithoutNotify);
            LunarConsole.instance.registry.Register(cVar);
#else
            Debug.LogError("Failed to register cheat property: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }

        public void Register(string name, Func<string> getter, Action<string> setter = null) {
#if LUNAR_CONSOLE
            if (!LunarConsole.isConsoleEnabled) {
                return;
            }

            var cVar          = new CVar(name, "", CFlags.NoArchive);
            var ignoreChanged = false;

            void SetValueWithoutNotify(string value) {
                var prevValue = cVar.Value;

                ignoreChanged   = true;
                cVar.Value = value;
                ignoreChanged   = false;

                if (prevValue != value) {
                    RefreshVariableInConsole(cVar);
                }
            }

            AddDelegate(this.lifetime, cVar, _ => {
                if (ignoreChanged) {
                    return;
                }

                var newValue = cVar.Value;
                setter?.Invoke(newValue);
                SetValueWithoutNotify(getter.Invoke());
            });
            Atom.Reaction(this.lifetime, () => getter.Invoke(), SetValueWithoutNotify);
            LunarConsole.instance.registry.Register(cVar);
#else
            Debug.LogError("Failed to register cheat property: Lunar Console not exist in project. Suggestion: add LUNAR_CONSOLE define");
#endif
        }

#if LUNAR_CONSOLE
        private static void AddDelegate(Lifetime lt, CVar var, CVarChangedDelegate call) {
            lt.Bracket(
                () => var.AddDelegate(call),
                () => var.RemoveDelegate(call)
            );
        }

        private static void RefreshVariableInConsole(CVar var) {
            var registry = LunarConsole.instance.registry;
            registry.registryDelegate?.OnVariableUpdated(registry, var);
        }
#endif
    }
}