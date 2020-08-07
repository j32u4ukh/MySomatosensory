using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

public interface INumList<T>
{
    int length();
    T sum();
    T mean(int digit = 4);
    void add(T val);
    void clear();
}

public class FloatList : INumList<float>
{
    List<float> numbers;

    public FloatList()
    {
        numbers = new List<float>();
    }

    public FloatList(params float[] numbers)
    {
        this.numbers = new List<float>(numbers);
    }

    public float this[int index]
    {
        set { numbers[index] = value; }
        get { return numbers[index]; }
    }

    public static FloatList operator +(FloatList list1, FloatList list2)
    {
        if(list1.length() != list2.length())
        {
            throw new Exception(string.Format("FloatList 相加的兩物件，個數需相同"));
        }

        int i, len = list1.length();
        float val;
        FloatList list = new FloatList();

        for(i = 0; i < len; i++)
        {
            val = list1[i] + list2[i];
            list.add(val);
        }

        return list;
    }

    public static FloatList operator -(FloatList list1, FloatList list2)
    {
        if (list1.length() != list2.length())
        {
            throw new Exception(string.Format("FloatList 相加的兩物件，個數需相同"));
        }

        int i, len = list1.length();
        float val;
        FloatList list = new FloatList();

        for (i = 0; i < len; i++)
        {
            val = list1[i] - list2[i];
            list.add(val);
        }

        return list;
    }

    public void add(float val)
    {
        numbers.Add(val);
    }

    public void clear()
    {
        numbers = new List<float>();
    }

    public int length()
    {
        return numbers.Count;
    }

    public float sum()
    {
        float _sum = 0f;
        foreach (float num in numbers)
        {
            _sum += num;
        }

        return _sum;
    }

    public float mean(int digit = 4)
    {
        if(length() == 0)
        {
            return 0f;
        }

        return (float)Math.Round(sum() / length(), digit);
    }

    public float geometricMean(int digit = 4)
    {
        float geometric = 1f;
        foreach (float num in numbers)
        {
            geometric *= num;
        }

        return (float)Math.Round(Math.Pow(geometric, 1f / length()), digit);
    }

    public string toString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");

        if (length() > 0)
        {
            int i;
            for (i = 0; i < length() - 1; i++)
            {
                sb.Append(string.Format("{0:F4}, ", numbers[i]));
            }
            sb.Append(string.Format("{0:F4}", numbers[i]));
        }

        sb.Append("]");
        
        return sb.ToString();
    }
}
