using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class GridLayoutWindow : EditorWindow
{
    GridLayout layout;
    Button[,] buttons;
    public void Init(GridLayout layout)
    {
        GridLayoutWindow window = (GridLayoutWindow)GetWindow(typeof(GridLayoutWindow));
        window.layout = layout;
        DrawGUI();
    }

    public void DrawGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/UI/Grid Editor/GridLayoutWindow.uxml");
        visualTree.CloneTree(root);
        var scrollView = root.Q<ScrollView>();

        if (layout == null)
        {
            layout = Selection.activeObject as GridLayout;
        }

        if (layout.Cells == null)
        {
            layout.InitCells();
        }
        else if (layout.Cells.Length != layout.rows * layout.columns)
        {
            layout.InitCells();
        }

        if (buttons == null)
        {
            buttons = new Button[layout.rows, layout.columns];
        }
        
        for (int y = layout.rows - 1; y >= 0; y--)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.width = scrollView.style.width;
            int yOffset = 50 * Mathf.Abs(y - (layout.rows - 1));
            int yIndex = y;
            for (int x = 0; x < layout.columns; x++)
            {
                int xOffset = 30 * x + 5;
                int xIndex = x;
                buttons[y, x] = new Button(() => { SetCellState(xIndex, yIndex); })
                {
                    text = $"({x}, {y})"
                };
                buttons[y, x].style.top = yOffset;
                buttons[y, x].style.left = xOffset;
                buttons[y, x].style.color = Color.black;
                buttons[y, x].style.height = 50;
                buttons[y, x].style.width = 50;
                buttons[y, x].style.display = DisplayStyle.Flex;

                switch (layout.GetCell(x, y).state)
                {
                    case CellInfo.CellState.Wall:
                        buttons[y, x].style.backgroundColor = Color.red;
                        break;
                    case CellInfo.CellState.GroundWall:
                        buttons[y, x].style.backgroundColor = Color.magenta;
                        break;
                    default:
                        buttons[y, x].style.backgroundColor = Color.white;
                        break;
                }
                row.Add(buttons[y, x]);
            }
            scrollView.Add(row);
        }
        root.Add(scrollView);
    }

    public void SetCellState(int x, int y)
    {
        CellInfo.CellState state = layout.SetCellState(new Vector2Int(x, y));
        switch (state)
        {
            case CellInfo.CellState.Wall:
                buttons[y, x].style.backgroundColor = Color.red;
                break;
            case CellInfo.CellState.GroundWall:
                buttons[y, x].style.backgroundColor = Color.magenta;
                break;
            default:
                buttons[y, x].style.backgroundColor = Color.white;
                break;
        }
        AssetDatabase.SaveAssets();
    }
}
