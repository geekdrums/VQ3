using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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
