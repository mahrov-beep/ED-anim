namespace Quantum
{
  public partial struct AIContextUser
  {
  }

  public static unsafe class AIContextExtensions
  {
    public static ref AIContextUser Data(this ref AIContext aiContext)
    {
      return ref *(AIContextUser*)aiContext.UserData;
    }
  }
}
