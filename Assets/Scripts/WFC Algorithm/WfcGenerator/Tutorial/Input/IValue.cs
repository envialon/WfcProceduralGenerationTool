using System;
using System.Collections.Generic;

namespace WFC_Tutorial
{
    public interface IValue<T> : IEqualityComparer<IValue<T>>, IEquatable<IValue<T>>
    {
        T Value { get; }
    }
}