using UnityEditor;
using UnityEditor.Experimental.GraphView;

public class GraphEditorWindow : EditorWindow
{
    [MenuItem("ProjectCustomTools/TestGraphEditor")]
    private static void OpenWindow()
    {
        var graphWindow = GetWindow<GraphEditorWindow>();
        graphWindow?.Initialize();
        graphWindow?.Show();
    }
    private GraphContainer m_graphContainer;

    public void Initialize()
    {
        titleContent = new UnityEngine.GUIContent("Render Graph");

        m_graphContainer = new GraphContainer();
        m_graphContainer.style.flexGrow = 1;
        rootVisualElement.Add(m_graphContainer);
        m_graphContainer.InitializeView(this);
        Repaint();
    }
}
