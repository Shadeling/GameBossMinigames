using UnityEngine;
using System.Collections.Generic;

namespace XD
{
    [System.Serializable]
    public class Clamper
    {
        [SerializeField] private float min = 0;
        [SerializeField] private float current = 0;
        [SerializeField] private float max = 0;

        public float Min { get => min; set => min = value; }
        public float Current { get => current; set => current = value; }
        public float Max { get => max; set => max = value; }
        public float Percent => GetPercent(Current);
        public float Difference => Max - Min;
        public Vector3 Vector3 => new Vector3(min, current, max);
        public bool Minimum => Min >= Current;
        public bool Peaked => Max <= Current;
        public static Clamper Zero => new Clamper(0);
        public bool IsZero => MiscTools.Approximately(Min, 0)
                              && MiscTools.Approximately(Current, 0)
                              && MiscTools.Approximately(Max, 0);


        public Clamper Clone()
        {
            return new Clamper(Min, Current, Max);
        }

        public Clamper Multiply(float multiplier)
        {
            Min *= multiplier;
            Current *= multiplier;
            Max *= multiplier;

            return this;
        }

        public Clamper Multiply(Clamper multiplier)
        {
            Min *= multiplier.Min;
            Current *= multiplier.Current;
            Max *= multiplier.Max;

            return this;
        }

        public Clamper Subtract(Clamper subtracter)
        {
            Min -= subtracter.Min;
            Current -= subtracter.Current;
            Max -= subtracter.Max;

            return this;
        }

        public void Summ(Clamper subtracter)
        {
            Min += subtracter.Min;
            Current += subtracter.Current;
            Max += subtracter.Max;
        }

        public Clamper()
        {
        }

        public Clamper(float value)
        {
            Min = 0;
            Current = value;
            Max = value;
        }

        public Clamper(float _current, float _max)
        {
            Min = 0;
            Current = _current;
            Max = _max;
        }

        public Clamper(float min, float current, float max)
        {
            Min = min;
            Current = current;
            Max = max;
        }

        public void Refresh()
        {
            Current = Max;
        }

        public void Reset()
        {
            Current = Min;
        }

        public float RandomValue()
        {
            return Random.Range(Min, Max);
        }

        public void Randomize()
        {
            Current = Random.Range(Min, Max); 
        }

        public void Clamp()
        {
            if (Peaked)
            {
                Refresh();
            }

            if (Minimum)
            {
                Reset();
            }
        }

        public void Plus(float value)
        {
            Current += value;
            Clamp();
        }

        public float GetByPercent(float percent)
        {
            return Min + (Max - Min) * percent;
        }

        public float GetPercent(float value)
        {
            if (Difference != 0 && (Max > Min))
                return (value - Min) / (Difference);

            return 0f;
        }

        public void AdditionByPercent(Clamper value)
        {
            Current += value.RandomValue() * Max;
            Clamp();
        }

        public bool Content(float value)
        {
            return value > Min && value <= Max;
        }

        public void Set(float value, bool expand = false)
        {
            if(expand && value > Max)
            {
                Max = value;
            }
            else if (expand && value < Min)
            {
                Min = value;
            }

            Current = Mathf.Clamp(value, Min, Max);
        }

        public void Set(Clamper value)
        {
            Min = value.Min;
            Current = value.Current;
            Max = value.Max;                 
        }

        public void Set(float _min, float _current, float _max)
        {
            Min = _min;
            Current = _current;
            Max = _max;
        }

        public static Clamper operator +(Clamper value, Clamper addition)
        {
            Clamper newValue = new Clamper(value.Min + addition.Min, value.Current + addition.Current, value.Max + addition.Max);
            newValue.Clamp();
            return newValue;
        }

        public static Clamper operator *(Clamper value, float koefficient)
        {
            value.Current *= koefficient;
            value.Clamp();
            return value;         
        }

        public static Clamper operator *(Clamper value, Clamper value2)
        {
            Clamper newValue = new Clamper(value.Min * value2.Min, value.Current * value2.Current, value.Max * value2.Max);
            newValue.Clamp();
            return newValue;
        }

        public static Clamper operator /(Clamper value, float koefficient)
        {
            value.Current /= koefficient;
            value.Clamp();
            return value;     
        }

        public static Clamper operator -(Clamper value, Clamper subtrahend)
        {
            Clamper newValue = new Clamper(value.Min - subtrahend.Min, value.Current - subtrahend.Current, value.Max - subtrahend.Max);
            newValue.Clamp();
            return newValue;
        }

        public static Clamper operator +(Clamper value, float _value2)
        {
            value.Current += _value2;
            value.Clamp();
            return value;            
        }

        public static Clamper operator -(Clamper value, float subtrahend)
        {
            value.Current -= subtrahend;
            value.Clamp();
            return value;
        }

        public static implicit operator float(Clamper value)
        {
            return value.Current;
        }

        public static implicit operator int(Clamper value)
        {
            return (int)value.Current;
        }

        public static implicit operator Vector2(Clamper value)
        {
            return new Vector2(value.Current, value.Max);
        }

        public static implicit operator Clamper(Dictionary<string, object> value)
        {
            float min = -1;
            float current = -1;
            float max = -1;

            value.Extract("min", ref min);
            value.Extract("current", ref current);
            value.Extract("max", ref max);

            return new Clamper(min, current, max);
        }

        public void ToInt()
        {
            Min = Mathf.CeilToInt(Min);
            Current = Mathf.CeilToInt(Current);
            Max = Mathf.CeilToInt(Max);
        }
        
        public override string ToString()
        {
            return "(" + Min.Round(4) + ", " + Current.Round(4) + ", " + Max.Round(4) + ")";
        }
    }
}