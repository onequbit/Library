using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Library
{

    public struct Maybe<T>
    {
        /*
            https://habr.com/en/post/458692/
        */
        
        public static implicit operator Maybe<T>(T value) => Value(value);

        public static Maybe<T>  Value(T value) => new Maybe<T>(false, value);

        public static readonly Maybe<T> Nothing = new Maybe<T>(true, default);

        private Maybe(bool isNothing, T value)
        {
            this.IsNothing = isNothing;
            this._value = value;
        }

        public readonly bool IsNothing;

        private readonly T _value;

        public T GetValue() => this.IsNothing ? throw new Exception("Nothing") : this._value;
    }

    public static class MaybeExtensions
    {
        public static Maybe<TRes> SelectMany<TIn, TRes>(
            this Maybe<TIn> source, 
            Func<TIn, Maybe<TRes>> func)

            => source.IsNothing ? 
                Maybe<TRes>.Nothing : 
                func(source.GetValue());
    }
}
