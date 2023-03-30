using System;
using UnityEngine;

public struct TimeSpan {
    public int Year;
    public int Month;

    public TimeSpan(int year, int month) {
        Year = year;
        Month = month;
    }

    public TimeSpan Next() {
        TimeSpan next = new TimeSpan(Year, Month);
        next.Month += 1;
        if (next.Month >= 12) {
            next.Year += 1;
            next.Month -= 12;
        }
        return next;
    }

    public override bool Equals(object obj)
    {
        TimeSpan? second = obj as TimeSpan?; 

        return second != null && this == (TimeSpan)second; 
    }

    public override int GetHashCode()
    {
        return (Year * 12 + Month).GetHashCode();
    }

    public static bool operator ==(TimeSpan left, TimeSpan right) {
        return left.Year == right.Year && left.Month == right.Month;
    }

    public static bool operator !=(TimeSpan left, TimeSpan right) {
        return left.Year != right.Year || left.Month != right.Month;
    }
}

public class LifeTime : MonoBehaviour {
    private TimeSpan _time;
    public TimeSpan Time => _time;

    public Action OnNextMonth;
    
    public string timeStr => $"{Time.Year + 1}年 {Time.Month + 1}月";

    public void NextMonth() {
        _time = _time.Next();
        OnNextMonth?.Invoke();
    }
}