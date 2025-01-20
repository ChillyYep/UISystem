using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IKUtility
{
    //public static IKNode[] CCD(IKNode[] nodes, Vector3 targetPos, int iteratorCount)
    //{
    //    if (nodes == null || nodes.Length <= 1)
    //    {
    //        return nodes;
    //    }
    //    IKNode[] resultNodes = new IKNode[nodes.Length];
    //    IKNode rootNode = nodes[0];
    //    IKNode leafNode = nodes[nodes.Length - 1];
    //    for (int i = 0; i < iteratorCount; ++i)
    //    {
    //        for (int j = nodes.Length - 1; j >= 0; j--)
    //        {
    //            IKNode curNode = nodes[j];
    //            IKNode prevNode = curNode.m_prevNode;
    //            if (prevNode != null)
    //            {
    //                prevNode.m_rotation = prevNode.m_rotation * Quaternion.FromToRotation(leafNode.m_position - prevNode.m_position, targetPos - prevNode.m_position);
    //            }
    //        }
    //    }
    //}

    //private static void UpdateAllChildNodePosition(IKNode node)
    //{
    //    IKNode curNode = node;
    //    while (curNode != null)
    //    {
    //        curNode = curNode.m_nextNode;
    //    }
    //}
}
