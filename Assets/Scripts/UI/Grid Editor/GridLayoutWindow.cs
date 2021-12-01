using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;

public class GridLayoutWindow : EditorWindow
{
    GridLayout layout;
    Vector2 scrollPosition = Vector2.zero;
    public static void Init(GridLayout layout)
    {
        GridLayoutWindow window = (GridLayoutWindow)GetWindow(typeof(GridLayoutWindow));
        window.layout = layout;
        window.Show();
    }

    public void OnGUI()
    {
        if (layout.cells == null)
        {
            layout.cells = new CellInfo[layout.rows, layout.columns];
        }
        else if (layout.cells.GetLength(0) != layout.rows || layout.cells.GetLength(1) != layout.columns)
        {
            CellInfo[,] cellsCopy = (CellInfo[,])layout.cells.Clone();
            layout.cells = new CellInfo[layout.rows, layout.columns];
            for (int y = 0; y < cellsCopy.GetLength(0); y++)
            {
                for (int x = 0; x < cellsCopy.GetLength(1); x++)
                {
                    layout.cells[y, x] = cellsCopy[y, x];
                }
            }
        }

        GUIStyle currentStyle;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.BeginVertical();

        for (int y = layout.rows - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < layout.columns; x++)
            {
                switch (layout.cells[y, x].state)
                {
                    case CellInfo.CellState.Wall:
                        currentStyle = new GUIStyle(GUI.skin.box);
                        currentStyle.fixedHeight = 50;
                        currentStyle.fixedWidth = 50;
                        currentStyle.normal.background = MakeTex(2, 2, Color.red);
                        break;
                    case CellInfo.CellState.GroundWall:
                        currentStyle = new GUIStyle(GUI.skin.box);
                        currentStyle.fixedHeight = 50;
                        currentStyle.fixedWidth = 50;
                        currentStyle.normal.background = MakeTex(2, 2, Color.blue);
                        break;
                    default:
                        currentStyle = new GUIStyle(GUI.skin.box);
                        currentStyle.fixedHeight = 50;
                        currentStyle.fixedWidth = 50;
                        currentStyle.normal.background = MakeTex(2, 2, Color.white);
                        break;
                }

                if (GUILayout.Button($"({x}, {y})", currentStyle))
                {
                    layout.SetCellState(new Vector2Int(x, y));
                }
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
