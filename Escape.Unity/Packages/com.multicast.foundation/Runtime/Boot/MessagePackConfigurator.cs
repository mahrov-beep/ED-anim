namespace Multicast.Boot {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DropSystem;
    using External.MessagePack.Unity;
    using External.MessagePack.Unity.Extension;
    using MessagePack;
    using MessagePack.Formatters;
    using MessagePack.Internal;
    using MessagePack.Resolvers;
    using Numerics;
    using UnityEngine;

    public class MessagePackConfigurator {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        private static void ConfigureMessagePack() {
            var resolvers = new List<IFormatterResolver> {
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance,
                UnityResolver.Instance,
                UnityBlitWithPrimitiveArrayResolver.Instance,
                SourceGeneratedFormatterResolver.Instance,
                //StandardResolver.Instance, // do NOT use dynamic resolver, not work on IL2CPP
            };
            
            // Добавляем sourceGen ресолверы вручную в дополнение к SourceGeneratedFormatterResolver
            // потому что SourceGeneratedFormatterResolver не предоставляет форматтеры для некоторых типов,
            // например для списков, таких как List<YourCustomType>
            resolvers.AddRange(AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => {
                    try {
                        return asm.GetCustomAttributes<GeneratedAssemblyMessagePackResolverAttribute>();
                    }
                    catch {
                        return Array.Empty<GeneratedAssemblyMessagePackResolverAttribute>();
                    }
                })
                .Select(attr => (IFormatterResolver) attr.ResolverType.GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null))
                .Where(it => it != null)
            );

            var formatters = new List<IMessagePackFormatter> {
                new DictionaryFormatter<string, string>(),
            };

            RegisterFormatters<bool>(formatters, null, Opt.Dict);
            RegisterFormatters<int>(formatters, null, Opt.Dict);
            RegisterFormatters<float>(formatters, null, Opt.Dict);
            RegisterFormatters<double>(formatters, null, Opt.Dict);
            RegisterFormatters<DateTime>(formatters, null, Opt.Dict);

            RegisterFormatters<Vector2>(formatters, null, Opt.Dict);
            RegisterFormatters<Vector3>(formatters, null, Opt.Dict);
            RegisterFormatters<Vector4>(formatters, null, Opt.Dict);
            RegisterFormatters<Vector2Int>(formatters, null, Opt.Dict);
            RegisterFormatters<Vector3Int>(formatters, null, Opt.Dict);

            RegisterFormatters(formatters, BigDoubleFormatter.Instance, Opt.Value | Opt.Nullable | Opt.Array | Opt.List | Opt.Dict);
            RegisterFormatters(formatters, FixedDoubleFormatter.Instance, Opt.Value | Opt.Nullable | Opt.Array | Opt.List | Opt.Dict);
            RegisterFormatters(formatters, ProtectedBigDoubleFormatter.Instance, Opt.Value | Opt.Nullable | Opt.Array | Opt.List | Opt.Dict);
            RegisterFormatters(formatters, ProtectedIntFormatter.Instance, Opt.Value | Opt.Nullable | Opt.Array | Opt.List | Opt.Dict);
            RegisterFormatters(formatters, GameTimeFormatter.Instance, Opt.Value | Opt.Nullable | Opt.Array | Opt.List | Opt.Dict);
            RegisterFormatters(formatters, DropFormatter.Instance, Opt.Value | Opt.Nullable | Opt.Array | Opt.List | Opt.Dict);

            StaticCompositeResolver.Instance.Register(formatters, resolvers);

            Verify(StaticCompositeResolver.Instance.GetFormatter<string>());
            Verify(StaticCompositeResolver.Instance.GetFormatter<string[]>());
            Verify(StaticCompositeResolver.Instance.GetFormatter<List<string>>());
            Verify(StaticCompositeResolver.Instance.GetFormatter<Dictionary<string, string>>());

            PreserveFormatter<bool>();
            PreserveFormatter<int>();
            PreserveFormatter<float>();
            PreserveFormatter<double>();
            PreserveFormatter<DateTime>();

            PreserveFormatter<Vector2>();
            PreserveFormatter<Vector3>();
            PreserveFormatter<Vector4>();
            PreserveFormatter<Vector2Int>();
            PreserveFormatter<Vector3Int>();

            PreserveFormatter<BigDouble>();
            PreserveFormatter<FixedDouble>();
            PreserveFormatter<ProtectedBigDouble>();
            PreserveFormatter<ProtectedInt>();
            PreserveFormatter<GameTime>();
            PreserveFormatter<Drop>();

            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
                .WithResolver(StaticCompositeResolver.Instance);
        }

        private static void RegisterFormatters<T>(List<IMessagePackFormatter> formatters,
            IMessagePackFormatter<T> formatter, Opt options)
            where T : struct {
            if ((options & Opt.Value) != 0) {
                if (formatter == null) {
                    throw new ArgumentNullException(nameof(formatter), typeof(T).FullName);
                }

                formatters.Add(formatter);
            }

            if ((options & Opt.Nullable) != 0) {
                formatters.Add(new NullableFormatter<T>());
            }

            if ((options & Opt.Array) != 0) {
                formatters.Add(new ArrayFormatter<T>());
            }

            if ((options & Opt.List) != 0) {
                formatters.Add(new ListFormatter<T>());
            }

            if ((options & Opt.Dict) != 0) {
                formatters.Add(new DictionaryFormatter<string, T>());
            }
        }

        private static void PreserveFormatter<T>() where T : struct {
            Verify(StaticCompositeResolver.Instance.GetFormatter<T>());
            Verify(StaticCompositeResolver.Instance.GetFormatter<T?>());
            Verify(StaticCompositeResolver.Instance.GetFormatter<T[]>());
            Verify(StaticCompositeResolver.Instance.GetFormatter<List<T>>());
            Verify(StaticCompositeResolver.Instance.GetFormatter<Dictionary<string, T>>());
        }

        private static void Verify<T>(IMessagePackFormatter<T> formatter) {
            if (formatter == null) {
                Debug.LogError($"[MessagePack] No formatter registered for '{typeof(T)}'");
            }
        }

        [Flags]
        private enum Opt {
            None     = 0,
            Value    = 1 << 0,
            Array    = 1 << 1,
            Nullable = 1 << 2,
            List     = 1 << 3,
            Dict     = 1 << 4,
        }
    }
}