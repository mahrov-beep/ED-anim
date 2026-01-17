namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Collections;
  using Photon.Deterministic;
  using Unity.IL2CPP.CompilerServices;

  [Il2CppSetOption(Option.NullChecks, false)]
  [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
  [Il2CppSetOption(Option.DivideByZeroChecks, false)]
  public unsafe struct FPBoostedCalculator : IDisposable {
    Frame                                               frame;
    List<QDictionaryPtr<EAttributeType, AttributeData>> attributeSources;

    public static FPBoostedCalculator Create(Frame f) {
      return new FPBoostedCalculator {
        frame            = f,
        attributeSources = GetList(),
      };
    }

    public void Dispose() {
      ReturnList(attributeSources);
    }

    public void AddSource(EntityRef e) {
      this.attributeSources.Add(frame.Unsafe.GetPointer<Attributes>(e)->DataDictionary);
    }

    public FPBoostedValue CalcAdditiveValue(FP baseValue, EAttributeType additive) {
      var value = (FPBoostedValue)baseValue;

      foreach (var attributeSource in attributeSources) {
        AttributesHelper.ApplyAsAdditiveValueOn(ref value, frame, attributeSource, additive);
      }

      return value;
    }

    public FPBoostedMultiplier CalcPercentMult(EAttributeType percent) {
      var value = FPBoostedMultiplier.One;

      foreach (var attributeSource in attributeSources) {
        AttributesHelper.ApplyAsPercentMultiplierOn(ref value, frame, attributeSource, percent);
      }

      return value;
    }

    static List<QDictionaryPtr<EAttributeType, AttributeData>> GetList() {
      return ThreadStaticCache<List<QDictionaryPtr<EAttributeType, AttributeData>>>.Get();
    }

    static void ReturnList(List<QDictionaryPtr<EAttributeType, AttributeData>> list) {
      list.Clear();
      ThreadStaticCache<List<QDictionaryPtr<EAttributeType, AttributeData>>>.Return(list);
    }

    static class ThreadStaticCache<T> where T : new() {
      [ThreadStatic] static Stack<T> items;

      public static T Get() {
        items ??= new Stack<T>();

        return items.Count > 0 ? items.Pop() : new T();
      }

      public static void Return(T item) {
        items.Push(item);
      }
    }
  }
}