using System;
using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    [Serializable]
    public class Date
    {
        [Range(1, 31)]
        public int day;
        [Range(1, 12)]
        public int month;
        [Range(2000, 2017)]
        public int year;

        public DateTime ToDateTime
        {
            get
            {
                return new DateTime(year, month, day);
            }
        }

        public Date SetFromDateTime(DateTime newDate)
        {
            day = newDate.Day;
            month = newDate.Month;
            year = newDate.Year;
            return this;
        }
    }
}