using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKNode
{
    public Quaternion m_rotation;
    public IKNode m_prevNode;
    public IKNode m_nextNode;
    public float m_distance2PrevNode;
    public float m_distance2NextNode;
    public float m_maxAngle;
}
