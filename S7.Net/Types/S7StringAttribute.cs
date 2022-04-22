namespace S7.Net.Types;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class S7StringAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="S7StringAttribute"/> class.
    /// </summary>
    /// <param name="type">The string type.</param>
    /// <param name="reservedLength">Reserved length of the string in characters.</param>
    /// <exception cref="ArgumentException">Please use a valid value for the string type</exception>
    public S7StringAttribute(S7StringType type, int reservedLength)
    {
        if (!Enum.IsDefined(typeof(S7StringType), type))
            throw new ArgumentException("Please use a valid value for the string type");

        this.Type = type;
        this.ReservedLength = reservedLength;
    }

    /// <summary>
    /// Gets the type of the string.
    /// </summary>
    /// <value>
    /// The string type.
    /// </value>
    public S7StringType Type { get; }

    /// <summary>
    /// Gets the reserved length of the string in characters.
    /// </summary>
    /// <value>
    /// The reserved length of the string in characters.
    /// </value>
    public int ReservedLength { get; }

    /// <summary>
    /// Gets the reserved length in bytes.
    /// </summary>
    /// <value>
    /// The reserved length in bytes.
    /// </value>
    public int ReservedLengthInBytes => Type == S7StringType.S7String ? ReservedLength + 2 : (ReservedLength * 2) + 4;
}

/// <summary>
/// String type.
/// </summary>
public enum S7StringType
{
    /// <summary>
    /// ASCII string.
    /// </summary>
    S7String = VarType.S7String,

    /// <summary>
    /// Unicode string.
    /// </summary>
    S7WString = VarType.S7WString
}