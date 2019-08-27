using System;
public class DestroyedObjectException : Exception
{
    public DestroyedObjectException(string message) : base(message) {
    }
    public DestroyedObjectException() : base("forbidden access to a destroyed object")
    {
    }
}

