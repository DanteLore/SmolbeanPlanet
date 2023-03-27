[System.Serializable]
public class ImpossibleCombinationException : System.Exception
{
    public ImpossibleCombinationException() { }
    public ImpossibleCombinationException(string message) : base(message) { }
    public ImpossibleCombinationException(string message, System.Exception inner) : base(message, inner) { }
    protected ImpossibleCombinationException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
