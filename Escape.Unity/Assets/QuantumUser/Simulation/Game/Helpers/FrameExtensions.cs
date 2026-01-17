using System;
using System.Diagnostics;

namespace Quantum {
  using Core;

  public static unsafe partial class FrameExtensions {
    public static ComponentFilterStruct<T> FilterStruct<T>(this Frame f, out T filterStruct, 
            ComponentSet without = default,
            ComponentSet any = default) 
            where T : unmanaged {
      
      filterStruct = default;
      var filter = f.Unsafe.FilterStruct<T>(without: without, any: any);
      return filter;
    }

    public static T* GetPointer<T>(this FrameBase f, EntityRef e) where T : unmanaged, IComponent {
      return f.Unsafe.GetPointer<T>(e);
    }

    public static T* GetPointer<T>(this Frame f, EntityRef e) where T : unmanaged, IComponent {
      return f.Unsafe.GetPointer<T>(e);
    }

    public static bool TryGetPointer<T>(this Frame f, EntityRef e, out T* p) where T : unmanaged, IComponent {
      return f.Unsafe.TryGetPointer(e, out p);
    }

    public static bool TryGetPointer<T>(this FrameBase f, EntityRef e, out T* p) where T : unmanaged, IComponent {
      return f.Unsafe.TryGetPointer(e, out p);
    }

    public static bool Without<T>(this Frame f, EntityRef e) where T : unmanaged, IComponent {
      return f.Has<T>(e) == false;
    }

    public static bool TryGetPointers<T1, T2>(this Frame f, EntityRef e, out T1* p1, out T2* p2)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent {

      p1 = null;
      p2 = null;

      return f.Unsafe.TryGetPointer(e, out p1) && f.Unsafe.TryGetPointer(e, out p2);
    }

    public static bool TryGetPointers<T1, T2>(this FrameBase f, EntityRef e, out T1* p1, out T2* p2)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent {

      p1 = null;
      p2 = null;

      return f.Unsafe.TryGetPointer(e, out p1) && f.Unsafe.TryGetPointer(e, out p2);
    }

    public static bool TryGetPointers<T1, T2, T3>(this Frame f, EntityRef e, out T1* p1, out T2* p2, out T3* p3)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent {

      p1 = null;
      p2 = null;
      p3 = null;

      return f.Unsafe.TryGetPointer(e, out p1) && f.Unsafe.TryGetPointer(e, out p2) && f.Unsafe.TryGetPointer(e, out p3);
    }

    public static bool TryGetPointers<T1, T2, T3>(this FrameBase f, EntityRef e, out T1* p1, out T2* p2, out T3* p3)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent {

      p1 = null;
      p2 = null;
      p3 = null;

      return f.Unsafe.TryGetPointer(e, out p1) && f.Unsafe.TryGetPointer(e, out p2) && f.Unsafe.TryGetPointer(e, out p3);
    }

    public static bool TryGetPointers<T1, T2, T3, T4>(this Frame f, EntityRef e, out T1* p1, out T2* p2, out T3* p3, out T4* p4)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent {

      p1 = null;
      p2 = null;
      p3 = null;
      p4 = null;

      return f.Unsafe.TryGetPointer(e, out p1) && f.Unsafe.TryGetPointer(e, out p2) && f.Unsafe.TryGetPointer(e, out p3) && f.Unsafe.TryGetPointer(e, out p4);
    }

    public static bool TryGetPointers<T1, T2>(this FrameThreadSafe f, EntityRef e, out T1* p1, out T2* p2)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent {

      p1 = null;
      p2 = null;

      if (!f.TryGetPointer<T1>(e, out var t1)) return false;
      if (!f.TryGetPointer<T2>(e, out var t2)) return false;

      p1 = t1;
      p2 = t2;

      return true;
    }

    public static bool TryGetPointers<T1, T2, T3>(this FrameThreadSafe f, EntityRef e, out T1* p1, out T2* p2, out T3* p3)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent {

      p1 = null;
      p2 = null;
      p3 = null;

      if (!f.TryGetPointer<T1>(e, out var t1)) return false;
      if (!f.TryGetPointer<T2>(e, out var t2)) return false;
      if (!f.TryGetPointer<T3>(e, out var t3)) return false;

      p1 = t1;
      p2 = t2;
      p3 = t3;

      return true;
    }

    public static T* GetOrAddPointer<T>(this Frame f, EntityRef e) where T : unmanaged, IComponent {
      if (!f.TryGetPointer<T>(e, out T* pointer)) {
        f.Add(e, out pointer);
      }

      return pointer;
    }

    public static bool AnyComponent<T>(this Frame f, Func<T, bool> predicate) where T : unmanaged, IComponent {
      var filter = f.Filter<T>();
      while (filter.Next(out _, out T component)) {
        if (predicate(component)) {
          return true;
        }
      }
      return false;
    }
  }
}