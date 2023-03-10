using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using WFC_Tutorial;

namespace WFC_Tutorial
{
    public class TileBaseValue : IValue<TileBase>
    {
        private IValue<TileBase> value;

        public TileBaseValue(IValue<TileBase> value)
        {
            this.value = value;
        }

        public TileBase Value => this.Value;

        public bool Equals(IValue<TileBase> x, IValue<TileBase> y)
        {
            return x == y;
        }

        public bool Equals(IValue<TileBase> other)
        {
            return other.Value == this.Value;
        }

        public int GetHashCode(IValue<TileBase> obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
    }
}