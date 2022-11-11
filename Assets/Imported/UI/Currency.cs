using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XD
{
    [System.Serializable]
    public class Currency
    {
        [SerializeField]
        private List<CurrencyValue> balanceList = new List<CurrencyValue>();

        private Dictionary<CurrencyType, CurrencyValue> balanceDict = new Dictionary<CurrencyType, CurrencyValue>();

        private bool Inited
        {
            get { return balanceList.Count == balanceDict.Count && balanceList.Count > 0; }
        }

        public int Count
        {
            get { return balanceList.Count; }
        }

        public CurrencyValue this[int ndx]
        {
            get { return balanceList[ndx]; }
        }

        public CurrencyValue this[CurrencyType type]
        {
            get
            {
                for (int i = 0; i < balanceList.Count; i++)
                {
                    if (balanceList[i].Type == type)
                    {
                        return balanceList[i];
                    }
                }

                CurrencyValue newVal = new CurrencyValue(type, 0);
                if (!balanceDict.ContainsKey(type))
                {
                    balanceList.Add(newVal);
                    balanceDict.Add(type, newVal);
                }
                return balanceDict[type];
            }
        }

        public void Add(CurrencyType type, long amount)
        {
            for (int i = 0; i < balanceList.Count; i++)
            {
                if (balanceList[i].Type == type)
                {
                    balanceList[i].SetAmount(amount);
                    return;
                }
            }

            balanceList.Add(new CurrencyValue(type, amount));
        }

        public long Silver
        {
            get
            {
                Init();
                return (long)this[CurrencyType.Silver].Amount;
            }
            set
            {
                Init();
                this[CurrencyType.Silver].SetAmount(value);
            }
        }

        public long Gold
        {
            get
            {
                Init();
                return (long)this[CurrencyType.Gold].Amount;
            }
            set
            {
                Init();
                this[CurrencyType.Gold].SetAmount(value);
            }
        }

        public long Expa
        {
            get
            {
                Init();
                return (long)this[CurrencyType.Exp].AmountDouble;
            }
            set
            {
                Init();
                this[CurrencyType.Exp].SetAmount(value);
            }
        }

        public long Camouflages
        {
            get
            {
                Init();
                return (long)this[CurrencyType.Cams].Amount;
            }
            set
            {
                Init();
                this[CurrencyType.Cams].SetAmount(value);
            }
        }

        public long Decals
        {
            get
            {
                Init();
                return (long)this[CurrencyType.Decals].Amount;
            }
            set
            {
                Init();
                this[CurrencyType.Decals].SetAmount(value);
            }
        }

        private void Init()
        {
            if (Inited)
            {
                return;
            }

            balanceDict.Clear();

            CurrencyValue curVal;
            for (int i = 0; i < balanceList.Count; i++)
            {
                curVal = balanceList[i];
                balanceDict[curVal.Type] = curVal;
            }
        }

        public void Clear()
        {
            balanceList.Clear();
            balanceDict.Clear();
        }

        /// <summary>
        /// Проверка - достаточно ли "денег/опыта".
        /// </summary>
        public bool HasEnough(CurrencyValue val) => balanceDict.TryGetValue(val.Type, out CurrencyValue cur) && cur.Amount >= val.Amount;

        public Currency Clone()
        {
            Currency clon = new Currency();

            for (int i = 0; i < balanceList.Count; i++)
            {
                clon.balanceList.Add(balanceList[i].Clone());
            }

            foreach (var pair in balanceDict)
            {
                clon.balanceDict.Add(pair.Key, pair.Value.Clone());
            }

            return clon;
        }

        public List<CurrencyValue> ToList()
        {
            return balanceList;
        }

        public Currency()
        {

        }
    }

    [System.Serializable]
    public class CurrencyValue
    {
#pragma warning disable 414 // used for human readable name in editor
        [SerializeField] private string name = "";
#pragma warning restore 414 // used for human readable name in editor
        [SerializeField] private CurrencyType type = CurrencyType.Silver;
        [SerializeField] private decimal amount = 0m;

        private string stringCurrency = string.Empty;

        public decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public double AmountDouble { get { return (double)amount; } }

        public CurrencyType Type
        {
            get { return type; }
            set { type = value; }
        }

        public bool IsSilver { get { return type == CurrencyType.Silver; } }
        public bool IsGold { get { return type == CurrencyType.Gold; } }

        public string StringCurrency
        {
            get
            {
                if (stringCurrency != string.Empty)
                {
                    return stringCurrency;
                }

                return type.ToString();
            }

            set
            {
                stringCurrency = value;
            }
        }



        public CurrencyValue()
        {
        }

        public CurrencyValue(object obj)
        {
            var dict = obj as Dictionary<string, object> ?? new Dictionary<string, object>();

            var currency = dict.ContainsKey("currency") ? Convert.ToString(dict["currency"]) : "Silver";
            var value = dict.ContainsKey("value") ? Convert.ToDecimal(dict["value"]) : 0L;

            type = (CurrencyType)Enum.Parse(typeof(CurrencyType), currency, true);
            amount = value;
            name = ToString();
        }

        public CurrencyValue(CurrencyType type, decimal amount)
        {
            this.type = type;
            this.amount = amount;
            name = ToString();
        }

        public void Multiply(decimal value)
        {
            amount *= value;
        }

        public void Multiply(double value)
        {
            amount *= (decimal)value;
        }

        public void SetAmount(decimal value)
        {
            amount = value;
        }

        public void SetAmount(long value)
        {
            amount = value;
        }

        public void SetType(CurrencyType value)
        {
            type = value;
        }

        public void ChangeAmount(decimal delta)
        {
            amount += delta;
        }

        public void ChangeAmount(long delta)
        {
            amount += delta;
        }

        public static CurrencyValue operator *(CurrencyValue one, decimal value)
        {
            return new CurrencyValue(one.type, (long)(one.Amount * value));
        }

        public static CurrencyValue operator *(CurrencyValue one, double value)
        {
            return new CurrencyValue(one.type, (long)(one.Amount * (decimal)value));
        }

        public CurrencyValue Clone()
        {
            return new CurrencyValue(Type, Amount);
        }

        public override string ToString() => $"{type}: {amount}";
    }
}