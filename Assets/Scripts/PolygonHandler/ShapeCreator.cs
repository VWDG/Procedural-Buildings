using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

public class ShapeCreator : MonoBehaviour
{
    public MeshFilter meshFilter;
    [HideInInspector]
    public List<Shape> shapes = new List<Shape>();

    [HideInInspector]
    public bool showShapesList;

    [HideInInspector]
    public List<bool> showPointsInShape = new List<bool>();
 
    public float handleRadius = .5f;

    public Color highlightColor = Color.red;
    public Color activeColor = Color.white;
    public Color deactivedColor = Color.grey;

    public LineType lineType = LineType.dotted;
    public LineType lineType_highlighted = LineType.solid;
    public HandleType handleType = HandleType.circle;

    CompositeShape compShape;

    public void UpdateMeshDisplay() 
    {
        //CompositeShape compShape = new CompositeShape(shapes);
        compShape = new CompositeShape(shapes);
        meshFilter.mesh = compShape.GetMesh();
        //Debug.Log("Anzahl an Polygonen: " + compShape.polygons.Length);
    }

    public Polygon[] GetPolygons() 
    {
        return compShape.polygons;
    }
}

public enum LineType
{
    solid,
    dotted
}

public enum HandleType
{
    sphere,
    circle
}
