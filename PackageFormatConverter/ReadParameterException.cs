using System;

public class ReadParameterException : Exception
{
    public ReadParameterException()
    {
    }

    public ReadParameterException(string message)
        : base(message)
    {
    }

    public ReadParameterException(string message, Exception inner)
        : base(message, inner)
    {
    }
}