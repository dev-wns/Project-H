using System;
using System.Collections.Generic;


public struct Const<Type>
{
    public Type value { get; private set; }

    public Const(Type value ) : this()
    {
        this.value = value;
    }
}