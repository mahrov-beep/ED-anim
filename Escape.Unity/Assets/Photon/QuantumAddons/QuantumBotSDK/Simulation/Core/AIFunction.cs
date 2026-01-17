namespace Quantum
{
	public class AIFunction : AIFunctionBase
	{
	}

	public abstract partial class AIFunction<T> : AIFunctionBase<T>
	{
		public virtual T Execute(Frame f, EntityRef e, ref AIContext c) { return default; }

		public override T Execute(FrameThreadSafe frame, EntityRef e, ref AIContext c)
		{
			return Execute((Frame)frame, e, ref c);
		}
	}
}