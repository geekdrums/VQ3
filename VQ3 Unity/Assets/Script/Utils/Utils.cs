using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Utils
{
    public static readonly char[] comma = new char[] { ',' };
    public static readonly char[] space = new char[] { ' ' };
}

public class Pair<T, U>
{
    public T first;
    public U second;

    public Pair( T t, U u )
    {
        first = t;
        second = u;
    }

    public X Get<X>()
        where X : class
    {
        if( first is X ) return first as X;
        else if( second is X ) return second as X;
        else return null;
    }
}
