using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SliceNewSplines {

    //Constructor
    public SliceNewSplines()
    {
        
    }

    public Vector3[,] Slice(int slices, int NumberOfSliceMembers, List<Vector3> listToSort)
    {
        List<Vector3> sortedList = HeapSort(listToSort);
        Vector3[,] slicedGrid = new Vector3[slices, NumberOfSliceMembers];
        int currentLeft = iLeftChild(0);
        int currentRight = iRightChild(0);
        //int currentParent = 0;
        slicedGrid[0, 0] = sortedList[0];

        for (int i = 0; i < slices; i++)
        {
            for (int j = 1; j < NumberOfSliceMembers; j++)
            {

            }
        }

        return slicedGrid;
    }

    private List<Vector3> HeapSort(List<Vector3> listToSort)
    {
        //Build-Max-Heap
        int heapSize = listToSort.Count;
        for (int p = (heapSize - 1) / 2; p >= 0; p--)
            MaxHeapify(listToSort, heapSize, p);

        for (int i = listToSort.Count - 1; i > 0; i--)
        {
            //Swap
            Vector3 temp = listToSort[i];
            listToSort[i] = listToSort[0];
            listToSort[0] = temp;

            heapSize--;
            MaxHeapify(listToSort, heapSize, 0);
        }

        return listToSort;

    }

    private void MaxHeapify(List<Vector3> unsortedList, int heapSize, int index)
    {
        int leftChildIndex = iLeftChild(index);
        int rightChildIndex = iRightChild(index);
        int larger = 0;

        if (leftChildIndex < heapSize && compareVector3ZXY(unsortedList[leftChildIndex], unsortedList[index]) > 0)
            larger = leftChildIndex;
        else
            larger = index;

        if (rightChildIndex < heapSize && compareVector3ZXY(unsortedList[rightChildIndex], unsortedList[larger]) > 0)
            larger = rightChildIndex;

        if (larger != index)
        {
            Vector3 temp = unsortedList[index];
            unsortedList[index] = unsortedList[larger];
            unsortedList[larger] = temp;

            MaxHeapify(unsortedList, heapSize, larger);
        }
    }


    private int iParent(int i)
    {
        return (int)Math.Floor((double)(i - 1) / 2.0);
    }
    private int iLeftChild(int i)
    {
        return 2 * i + 1;
    }
    private int iRightChild(int i)
    {
        return 2 * i + 2;
    }

    private int compareVector3ZXY(Vector3 v1, Vector3 v2)
    {
        int resultZ = v1.z.CompareTo(v2.z);
        if (resultZ == 0)
        {
            int resultX = v1.x.CompareTo(v2.x);
            if (resultX == 0)
            {
                return v1.y.CompareTo(v2.y);
            }
            else return resultZ;
        }
        else return resultZ;
    }

}
