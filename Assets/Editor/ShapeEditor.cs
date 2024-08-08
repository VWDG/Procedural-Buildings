using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Geometry;

[CustomEditor(typeof(ShapeCreator))]
public class ShapeEditor : Editor
{
    ShapeCreator shapeCreator;
    SelectionInfo selectionInfo;
    bool shapeChangedSinceLastRepaint;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        string helpMessage = "Left click to add points. \nShift-left click on point to delete.\nShift-left click on empty space to create a new shape.";
        EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

        while (shapeCreator.showPointsInShape.Count < shapeCreator.shapes.Count) 
        {
            shapeCreator.showPointsInShape.Add(false);
        }
        while (shapeCreator.showPointsInShape.Count > shapeCreator.shapes.Count)
        {
            shapeCreator.showPointsInShape.RemoveAt(shapeCreator.showPointsInShape.Count-1);
        }

        Color defaultColor = GUI.color;

        int shapeDeleteIndex = -1;
        shapeCreator.showShapesList = EditorGUILayout.Foldout(shapeCreator.showShapesList, "Show Shapes List");
        if (shapeCreator.showShapesList)
        {
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                GUIStyle gui_style = new GUIStyle();
                gui_style.fontStyle = (i != selectionInfo.selectedShapeIndex)?FontStyle.Normal:FontStyle.Bold;
                gui_style.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

                GUIStyle gui_style_foldout = new GUIStyle(EditorStyles.foldout);
                gui_style_foldout.fontStyle = (i != selectionInfo.selectedShapeIndex) ? FontStyle.Normal : FontStyle.Bold;
                gui_style_foldout.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

                if (i % 2 == 0)
                {
                    //GUI.backgroundColor = Color.green;
                    GUILayout.BeginHorizontal(BackgroundStyle.Get(new Color(0.2f, 0.2f, 0.2f)));
                }
                else 
                {
                    //GUI.backgroundColor = defaultColor;
                    GUILayout.BeginHorizontal();
                }
                //GUILayout.BeginHorizontal();
                GUILayout.Label("Shape " + (i + 1), gui_style);
                GUI.enabled = i != selectionInfo.selectedShapeIndex;
                if (GUILayout.Button("Select"))
                {
                    selectionInfo.selectedShapeIndex = i;
                }
                GUI.enabled = true;
                if (GUILayout.Button("Delete"))
                {
                    shapeDeleteIndex = i;
                }
                GUILayout.EndHorizontal();
                if (i % 2 == 0)
                {
                    //GUI.backgroundColor = Color.green;
                    GUILayout.BeginVertical(BackgroundStyle.Get(new Color(0.2f, 0.2f, 0.2f)));
                }
                else
                {
                    //GUI.backgroundColor = defaultColor;
                    GUILayout.BeginVertical();
                }
                EditorGUI.indentLevel++;
                shapeCreator.showPointsInShape[i] = EditorGUILayout.Foldout(shapeCreator.showPointsInShape[i], "Show Points", gui_style_foldout);
                if (shapeCreator.showPointsInShape[i])
                {
                    Shape shape = shapeCreator.shapes[i];
                    for (int k = 0; k < shape.points.Count; k++)
                    {
                        shape.points[k] = EditorGUILayout.Vector3Field("point " + (k + 1) + ": ", shape.points[k]);
                        //Deactivate y point movement TODO: better?
                        shape.points[k] = new Vector3(shape.points[k].x, 0, shape.points[k].z);
                    }
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
                //GuiLine(1, Color.black);
            }
        }
        GUI.backgroundColor = defaultColor;

        if (shapeDeleteIndex != -1) 
        {
            Undo.RecordObject(shapeCreator, "Delete shape");
            shapeCreator.shapes.RemoveAt(shapeDeleteIndex);
            shapeCreator.showPointsInShape.RemoveAt(shapeDeleteIndex);
            selectionInfo.selectedShapeIndex = Mathf.Clamp(selectionInfo.selectedShapeIndex, 0, shapeCreator.shapes.Count-1);
        }

        if (GUI.changed) 
        {
            shapeChangedSinceLastRepaint = true;
            SceneView.RepaintAll();
        }
    }

    void GuiLine(int i_height, Color c)

    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, c);
    }

    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
            if (shapeChangedSinceLastRepaint)
            {
                HandleUtility.Repaint();
            }
        }
    }

    void Draw()
    {
        for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
        {
            Shape shapeToDraw = shapeCreator.shapes[shapeIndex];
            bool shapeIsSelected = shapeIndex == selectionInfo.selectedShapeIndex;
            bool mouseIsOverShape = shapeIndex == selectionInfo.mouseOverShapeIndex;
            Color deselectedShapeColour = shapeCreator.deactivedColor;

            for (int i = 0; i < shapeToDraw.points.Count; i++)
            {
                Vector3 nextPoint = shapeToDraw.points[(i + 1) % shapeToDraw.points.Count];
                if (i == selectionInfo.lineIndex && mouseIsOverShape)
                {
                    Handles.color = shapeCreator.highlightColor;
                    switch (shapeCreator.lineType_highlighted) 
                    {
                        case LineType.solid:
                            Handles.DrawLine(shapeToDraw.points[i], nextPoint);
                            break;
                        case LineType.dotted:
                            Handles.DrawDottedLine(shapeToDraw.points[i], nextPoint, 4);
                            break;
                    }
                    

                }
                else
                {
                    Handles.color = (shapeIsSelected)?shapeCreator.activeColor:deselectedShapeColour;
                    switch (shapeCreator.lineType)
                    {
                        case LineType.solid:
                            Handles.DrawLine(shapeToDraw.points[i], nextPoint);
                            break;
                        case LineType.dotted:
                            Handles.DrawDottedLine(shapeToDraw.points[i], nextPoint, 4);
                            break;
                    }
                }

                if (i == selectionInfo.pointIndex && mouseIsOverShape)
                {
                    Handles.color = (selectionInfo.pointIsSelected) ? shapeCreator.activeColor : shapeCreator.highlightColor;
                }
                else
                {
                    Handles.color = (shapeIsSelected)? shapeCreator.activeColor : deselectedShapeColour;
                }
                switch (shapeCreator.handleType)
                {
                    case HandleType.circle:
                        Handles.DrawSolidDisc(shapeToDraw.points[i], Vector3.up,  shapeCreator.handleRadius);
                        break;
                    case HandleType.sphere:
                        Handles.SphereHandleCap(0, shapeToDraw.points[i], Quaternion.identity, shapeCreator.handleRadius, EventType.Repaint);
                        break;
                }
               
            }
        }

        if (shapeChangedSinceLastRepaint) 
        {
            shapeCreator.UpdateMeshDisplay();
        }

        shapeChangedSinceLastRepaint = false;
    }

    void CreateNewPoint(Vector3 position)
    {
        bool mouseIsOverSelectedShape = selectionInfo.mouseOverShapeIndex == selectionInfo.selectedShapeIndex;
        int newPointIndex = (selectionInfo.mouseIsOverLine && mouseIsOverSelectedShape) ? selectionInfo.lineIndex + 1 : SelectedShape.points.Count;
        Undo.RecordObject(shapeCreator, "Add point");
        SelectedShape.points.Insert(newPointIndex, position);
        selectionInfo.pointIndex = newPointIndex;
        selectionInfo.mouseOverShapeIndex = selectionInfo.selectedShapeIndex;
        shapeChangedSinceLastRepaint = true;

        SelectPointUnderMouse();
    }

    void DeletePointUnderMouse() 
    {
        Undo.RecordObject(shapeCreator, "Delete point");
        SelectedShape.points.RemoveAt(selectionInfo.pointIndex);
        selectionInfo.pointIsSelected = false;
        selectionInfo.mouseIsOverPoint = false;
        shapeChangedSinceLastRepaint = true;
    }

    void SelectPointUnderMouse()
    {
        selectionInfo.pointIsSelected = true;
        selectionInfo.mouseIsOverPoint = true;
        selectionInfo.mouseIsOverLine = false;
        selectionInfo.lineIndex = -1;

        selectionInfo.positionAtStartOfDrag = SelectedShape.points[selectionInfo.pointIndex];
        shapeChangedSinceLastRepaint = true;
    }

    void SelectShapeUnderMouse() 
    {
        if (selectionInfo.mouseOverShapeIndex != -1) 
        {
            selectionInfo.selectedShapeIndex = selectionInfo.mouseOverShapeIndex;
            shapeChangedSinceLastRepaint = true;
        }
    }

    void CreateNewShape() 
    {
        Undo.RecordObject(shapeCreator, "Create shape");
        shapeCreator.shapes.Add(new Shape());
        selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
    }

    void HandleInput(Event guiEvent) 
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeight = 0;
        float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            HandleShiftLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
        {
            HandleLeftMouseUp(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDrag(mousePosition);
        }

        if (!selectionInfo.pointIsSelected)
        {
            UpdateMouseOverInfo(mousePosition);
        }
    }

    void HandleShiftLeftMouseDown(Vector3 mousePosition) 
    {
        if (selectionInfo.mouseIsOverPoint)
        {
            SelectShapeUnderMouse();
            DeletePointUnderMouse();
        }
        else
        {
            CreateNewShape();
            CreateNewPoint(mousePosition);
        }
    }

    void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if (shapeCreator.shapes.Count == 0) 
        {
            CreateNewShape();
        }

        SelectShapeUnderMouse();

        if (selectionInfo.mouseIsOverPoint)
        {
            SelectPointUnderMouse();
        }
        else 
        {
            CreateNewPoint(mousePosition);
        }
    }

    void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if (selectionInfo.pointIsSelected)
        {
            SelectedShape.points[selectionInfo.pointIndex] = selectionInfo.positionAtStartOfDrag;
            Undo.RecordObject(shapeCreator, "Move point");
            SelectedShape.points[selectionInfo.pointIndex] = mousePosition;

            selectionInfo.pointIsSelected = false;
            selectionInfo.pointIndex = -1;
            shapeChangedSinceLastRepaint = true;
        }
    }

    void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (selectionInfo.pointIsSelected) 
        {
            SelectedShape.points[selectionInfo.pointIndex] = mousePosition;
            shapeChangedSinceLastRepaint = true;
        }
    }

    void UpdateMouseOverInfo(Vector3 mousePosition) 
    {
        int mouseOverPointIndex = -1;
        int mouseOverShapeIndex = -1;

        for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
        {
            Shape currentShape = shapeCreator.shapes[shapeIndex];

            for (int i = 0; i < currentShape.points.Count; i++)
            {
                if (Vector3.Distance(mousePosition, currentShape.points[i]) < shapeCreator.handleRadius)
                {
                    mouseOverPointIndex = i;
                    mouseOverShapeIndex = shapeIndex;
                    break;
                }
            }
        }

        if (mouseOverPointIndex != selectionInfo.pointIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex) 
        {
            selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;
            selectionInfo.pointIndex = mouseOverPointIndex;
            selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

            shapeChangedSinceLastRepaint = true;
        }

        if (selectionInfo.mouseIsOverPoint)
        {
            selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;
        }
        else 
        {
            int mouseOverLineIndex = -1;
            float closestLineDst = shapeCreator.handleRadius;

            for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
            {
                Shape currentShape = shapeCreator.shapes[shapeIndex];

                for (int i = 0; i < currentShape.points.Count; i++)
                {
                    Vector3 nextPointInShape = currentShape.points[(i + 1) % currentShape.points.Count];
                    float dstFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePosition.ToXZ(), currentShape.points[i].ToXZ(), nextPointInShape.ToXZ());
                    if (dstFromMouseToLine < closestLineDst)
                    {
                        closestLineDst = dstFromMouseToLine;
                        mouseOverLineIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                    }
                }
            }

            if (selectionInfo.lineIndex != mouseOverLineIndex || mouseOverShapeIndex!= selectionInfo.mouseOverShapeIndex) 
            {
                selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;

                selectionInfo.lineIndex = mouseOverLineIndex;
                selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;
                shapeChangedSinceLastRepaint = true;
            }
        }
    }

    void OnEnable()
    {
        shapeChangedSinceLastRepaint = true;
        shapeCreator = target as ShapeCreator;
        selectionInfo = new SelectionInfo();
        Undo.undoRedoPerformed += OnUndoOrRedo;
        Tools.hidden = true;
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
        Tools.hidden = false;
    }

    void OnUndoOrRedo() 
    {
        if (selectionInfo.selectedShapeIndex >= shapeCreator.shapes.Count || selectionInfo.selectedShapeIndex == -1) 
        {
            selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
        }
        shapeChangedSinceLastRepaint = true;
    }

    Shape SelectedShape 
    {
        get 
        {
            return shapeCreator.shapes[selectionInfo.selectedShapeIndex];
        }
    }
    public class SelectionInfo 
    {
        public int selectedShapeIndex;
        public int mouseOverShapeIndex;

        public int pointIndex = -1;
        public bool mouseIsOverPoint;
        public bool pointIsSelected;
        public Vector3 positionAtStartOfDrag;

        public int lineIndex = -1;
        public bool mouseIsOverLine;
    
    
    }

    public static class BackgroundStyle
    {
        private static GUIStyle style = new GUIStyle();
        private static Texture2D texture = new Texture2D(1, 1);


        public static GUIStyle Get(Color color)
        {
            texture.SetPixel(0, 0, color);
            texture.Apply();
            style.normal.background = texture;
            return style;
        }
    }
}
