using NPOI.OpenXmlFormats.Dml;
using NPOI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public static class SortUtils
{
    private static void MergeSortAux(int[] originalArr, int leftIndex, int count, int leftCount)
    {
        int[] leftArr = new int[leftCount];
        int[] rightArr = new int[count - leftCount];
        int left = leftIndex;
        int right = leftIndex + leftCount;
        Array.Copy(originalArr, left, leftArr, 0, leftArr.Length);
        Array.Copy(originalArr, right, rightArr, 0, rightArr.Length);

        int i = 0, j = 0;
        while (i < leftArr.Length || j < rightArr.Length)
        {
            if (j >= rightArr.Length)
            {
                originalArr[left + i + j] = leftArr[i];
                i++;
            }
            else if (i >= leftArr.Length)
            {
                originalArr[left + i + j] = rightArr[j];
                j++;
            }
            else
            {
                if (leftArr[i] <= rightArr[j])
                {
                    originalArr[left + i + j] = leftArr[i];
                    i++;
                }
                else
                {
                    originalArr[left + i + j] = rightArr[j];
                    j++;

                }
            }
        }

    }
    private static void MergeSort(int[] originalArr, int startIndex, int count)
    {
        if (count == 1)
        {
            return;
        }
        int leftCount = count / 2;
        MergeSort(originalArr, startIndex, leftCount);
        MergeSort(originalArr, startIndex + leftCount, count - leftCount);
        MergeSortAux(originalArr, startIndex, count, leftCount);
    }
    /// <summary>
    /// Log2(n)�㷨���Ӷ�
    /// </summary>
    /// <param name="originalArr"></param>
    public static void MergeSort(int[] originalArr)
    {
        MergeSort(originalArr, 0, originalArr.Length);
    }
    /// <summary>
    /// n*n�㷨���Ӷ�
    /// </summary>
    /// <param name="originalArr"></param>
    public static void InsertionSort(int[] originalArr)
    {
        for (int i = 1; i < originalArr.Length; ++i)
        {
            int curValue = originalArr[i];
            for (int j = 0; j < i; ++j)
            {
                if (curValue < originalArr[j])
                {
                    // MoveBackward
                    for (int x = i; x > j; --x)
                    {
                        originalArr[x] = originalArr[x - 1];
                    }
                    originalArr[j] = curValue;
                    break;
                }
            }
        }
    }

    private static void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }
    private static void QuickSortAux(int[] originalArr, int startIndex, int endIndex)
    {
        if (startIndex >= endIndex)
        {
            return;
        }
        int left = startIndex;
        int right = endIndex;
        // ȡ�����Ϊpivot
        int pivot = originalArr[startIndex];
        // ��֤leftǰ��������С�ڵ���pivot�ģ�left��������Ǵ���pivot��
        while (left < right)
        {
            while (originalArr[left] <= pivot && left < right)
            {
                left++;
            }
            while (originalArr[right] >= pivot && left < right)
            {
                right--;
            }
            if (left < right)
            {
                Swap(ref originalArr[left], ref originalArr[right]);
            }
        }
        // �˴������������Һ�����������ҵ�˳�����޹ؽ�Ҫ
        left = originalArr[left] > pivot && left > startIndex ? left - 1 : left;
        // ����pivot��left
        Swap(ref originalArr[startIndex], ref originalArr[left]);

        QuickSortAux(originalArr, startIndex, left - 1);
        QuickSortAux(originalArr, left + 1, endIndex);
    }
    public static void QuickSort(int[] originalArr)
    {
        QuickSortAux(originalArr, 0, originalArr.Length - 1);
    }

    private static int GetParentIndex(int index)
    {
        return (index - 1) / 2;
    }
    private static int GetLeftChildIndex(int index)
    {
        return 2 * index + 1;
    }
    private static int GetRightChildIndex(int index)
    {
        return 2 * index + 2;
    }
    private static void MaxHeap(int[] originalArr, int heapSize, int index)
    {
        int leftChildIndex = GetLeftChildIndex(index);
        int rightChildIndex = GetRightChildIndex(index);
        int largestIndex = index;
        if (leftChildIndex < heapSize && originalArr[leftChildIndex] > originalArr[largestIndex])
        {
            largestIndex = leftChildIndex;
        }

        if (rightChildIndex < heapSize && originalArr[rightChildIndex] > originalArr[largestIndex])
        {
            largestIndex = rightChildIndex;
        }
        if (largestIndex != index)
        {
            Swap(ref originalArr[index], ref originalArr[largestIndex]);
            // ���������������Ͳ��������ѣ������Ҫ���������µ���Ϊ����
            MaxHeap(originalArr, heapSize, largestIndex);
        }
    }

    private static void BuildMaxHeap(int[] originalArr)
    {
        int startIndex = GetParentIndex(originalArr.Length - 1);
        for (int i = startIndex; i >= 0; i--)
        {
            MaxHeap(originalArr, originalArr.Length, i);
        }
    }
    public static void HeapSort(int[] originalArr)
    {
        // �״ι�������.n*log(n)
        BuildMaxHeap(originalArr);
        // n*log(n)
        for (int i = originalArr.Length - 1; i > 0; --i)
        {
            // �������ֵ���ѵ�
            Swap(ref originalArr[0], ref originalArr[i]);
            // ���¹���һ�����ѣ����Ѷ�Ԫ���ƶ��ƶ�������λ�ã�ʹ�ø�Ԫ�ر��ӽڵ㶼������ļ��������ǳ���,log(n)
            MaxHeap(originalArr, i, 0);
        }
    }

    //public static void ShellSort(int[] originalArr, out int[] resultArr) { }
}
public class SortReview : MonoBehaviour
{
    int[] arr = new int[] {
        1,5,8,8,7,3,5,6,7,8,1,23,5,4,23,5,4,8,6,3,65,78,13,45,89,43,0,4
    };
    // Start is called before the first frame update
    void Start()
    {
        SortUtils.HeapSort(arr);
        foreach (var element in arr)
        {
            Debug.Log(element);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
