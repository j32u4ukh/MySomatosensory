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
                sb.Append(numbers[i] + ", ");
            }
            sb.Append(numbers[i]);
        }

        sb.Append("]");
        
        return sb.ToString();
    }
}
