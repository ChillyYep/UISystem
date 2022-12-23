using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
public class CustomUserData
{
    public int m_id;
}
public class GraphNode : Node
{
    private Port[] m_inputPorts;
    private Port[] m_outputPorts;

    public void Initialize(float x, float y, string lable)
    {
        StyleSheet rn = Resources.Load<StyleSheet>("RenderNode");
        styleSheets.Add(rn);
        StyleSheet rp = Resources.Load<StyleSheet>("Port");
        styleSheets.Add(rp);
        SetPosition(new Rect(x, y, 0f, 0f));
        base.expanded = true;
        RefreshExpandedState();
    }

    public void UpdatePorts(string[] inputs, string[] outputs,IEdgeConnectorListener edgeConnectorListener)
    {
        inputContainer.Clear();
        outputContainer.Clear();
        m_inputPorts = new Port[inputs.Length];
        m_outputPorts = new Port[outputs.Length];
        for (int i = 0; i < inputs.Length; ++i)
        {
            m_inputPorts[i] = Port.Create<Edge>(Orientation.Vertical, Direction.Input, Port.Capacity.Single, null);
            m_inputPorts[i].portName = inputs[i];
            //m_inputPorts[i].userData = new CustomUserData() { m_id = i };
            m_inputPorts[i].AddManipulator(new CustomEdgeConnector(edgeConnectorListener));
            //m_inputPorts[i].portType = typeof(CustomUserData);
            //m_inputPorts[i].visualClass = "string";
            inputContainer.Add(m_inputPorts[i]);
        }
        for (int i = 0; i < outputs.Length; ++i)
        {
            m_outputPorts[i] = Port.Create<Edge>(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, null);
            m_outputPorts[i].portName = outputs[i];
            m_outputPorts[i].AddManipulator(new CustomEdgeConnector(edgeConnectorListener));
            //m_outputPorts[i].visualClass = "string";
            //m_outputPorts[i].userData = new CustomUserData() { m_id = i };
            //m_outputPorts[i].portType = typeof(CustomUserData);
            outputContainer.Add(m_outputPorts[i]);
        }
    }
}
