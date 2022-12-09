using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class EdgeConnectorListener : IEdgeConnectorListener
{
    public void OnDrop(GraphView graphView, Edge edge)
    {
        if (graphView is GraphContainer graphContainer)
        {
            edge.output.Connect(edge);
            edge.input.Connect(edge);
            graphContainer.AddElement(edge);
        }
    }

    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
    }
}

public class GraphContainer : GraphView
{
    private GraphEditorWindow m_parent;
    private EdgeConnectorListener m_edgeConnectorListener;

    public void InitializeView(GraphEditorWindow parent)
    {
        m_parent = parent;

        // Manipulators
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new RectangleSelector());

        // background
        StyleSheet styleSheet = Resources.Load<StyleSheet>("RenderGraphGridBackground");
        styleSheets.Add(styleSheet);
        GridBackground gridBackground = new GridBackground();
        // 添加完后要设置成父节点子节点列表中的首位，后续节点的AddElement可能是添加到一个默认的VisualElement中（这个VisualElement排在首位）
        //gridBackground.SendToBack();
        Insert(0, gridBackground);
        gridBackground.StretchToParentSize();

        // zoom
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        // 
        m_edgeConnectorListener = new EdgeConnectorListener();

        // searchWindow
        GraphSearchWindowProvider graphSearchWindowProvider = ScriptableObject.CreateInstance<GraphSearchWindowProvider>();
        graphSearchWindowProvider.Setup(nodeInfo =>
        {
            var node = new GraphNode();
            var windowRelativePos = nodeInfo.m_screenPos - m_parent.position.position;
            var localPos = contentViewContainer.WorldToLocal(windowRelativePos);
            // AddElement能够使节点绑定在容器坐标空间里，用Add则是和窗口空间同步
            AddElement(node);
            node.Initialize(localPos.x, localPos.y, nodeInfo.m_name);
            node.UpdatePorts(nodeInfo.m_inputPort, nodeInfo.m_outputPort, m_edgeConnectorListener);
        });
        nodeCreationRequest += context =>
        {
            var pos = context.screenMousePosition;
            SearchWindow.Open(new SearchWindowContext(pos), graphSearchWindowProvider);
        };


    }
}
