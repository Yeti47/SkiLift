namespace SkiLift;

/// <summary>
/// Absence of a type. 
/// This is used to represent the absence of a value in cases where a response type is required but no value is needed.
/// </summary>
public sealed class Void 
{  
    /// <summary>
    /// Singleton instance of <see cref="Void"/>.
    /// </summary>
    public static readonly Void Instance = new();

    private Void() { }
}