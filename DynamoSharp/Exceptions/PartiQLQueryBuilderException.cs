namespace DynamoSharp.Exceptions;

public class PartiQLQueryBuilderException : Exception
{
    public PartiQLQueryBuilderException(string message) : base(message) { }
}

public class MethodCallArgumentNullException : PartiQLQueryBuilderException
{
    public MethodCallArgumentNullException(string message) : base(message) { }
}

public class MethodCallObjectNullException : PartiQLQueryBuilderException
{
    public MethodCallObjectNullException(string message) : base(message) { }
}

public class MemberExpressionClosureNullException : PartiQLQueryBuilderException
{
    public MemberExpressionClosureNullException(string message) : base(message) { }
}

public class ConstantNullException : PartiQLQueryBuilderException
{
    public ConstantNullException(string message) : base(message) { }
}

public class ConstantValueNullException : PartiQLQueryBuilderException
{
    public ConstantValueNullException(string message) : base(message) { }
}

public class EnumValueNullException : PartiQLQueryBuilderException
{
    public EnumValueNullException(string message) : base(message) { }
}