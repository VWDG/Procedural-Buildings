using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 * Processes given arrays of hull and hole points into single array, enforcing correct -wiseness.
 * Also provides convenience methods for accessing different hull/hole points
 */

namespace Geometry
{
    public class Polygon
    {

        public readonly Vector2[] points;
        public readonly int numPoints;

        public readonly int numHullPoints;

        public readonly int[] numPointsPerHole;
        public readonly int numHoles;

        public readonly int[] holeStartIndices;

        public Polygon(Vector2[] hull, Vector2[][] holes)
        {
            numHullPoints = hull.Length;
            numHoles = holes.GetLength(0);

            numPointsPerHole = new int[numHoles];
            holeStartIndices = new int[numHoles];
            int numHolePointsSum = 0;

            for (int i = 0; i < holes.GetLength(0); i++)
            {
                numPointsPerHole[i] = holes[i].Length;

                holeStartIndices[i] = numHullPoints + numHolePointsSum;
                numHolePointsSum += numPointsPerHole[i];
            }

            numPoints = numHullPoints + numHolePointsSum;
            points = new Vector2[numPoints];


            // add hull points, ensuring they wind in counterclockwise order
            bool reverseHullPointsOrder = !PointsAreCounterClockwise(hull);
            for (int i = 0; i < numHullPoints; i++)
            {
                points[i] = hull[(reverseHullPointsOrder) ? numHullPoints - 1 - i : i];
            }

            // add hole points, ensuring they wind in clockwise order
            for (int i = 0; i < numHoles; i++)
            {
                bool reverseHolePointsOrder = PointsAreCounterClockwise(holes[i]);
                for (int j = 0; j < holes[i].Length; j++)
                {
                    points[IndexOfPointInHole(j, i)] = holes[i][(reverseHolePointsOrder) ? holes[i].Length - j - 1 : j];
                }
            }

        }

        public Polygon(Vector2[] hull) : this(hull, new Vector2[0][])
        {
        }

        bool PointsAreCounterClockwise(Vector2[] testPoints)
        {
            float signedArea = 0;
            for (int i = 0; i < testPoints.Length; i++)
            {
                int nextIndex = (i + 1) % testPoints.Length;
                signedArea += (testPoints[nextIndex].x - testPoints[i].x) * (testPoints[nextIndex].y + testPoints[i].y);
            }

            return signedArea < 0;
        }

        public int IndexOfFirstPointInHole(int holeIndex)
        {
            return holeStartIndices[holeIndex];
        }

        public int IndexOfPointInHole(int index, int holeIndex)
        {
            return holeStartIndices[holeIndex] + index;
        }

        public Vector2 GetHolePoint(int index, int holeIndex)
        {
            return points[holeStartIndices[holeIndex] + index];
        }

        public bool PointInsidePolygon(Vector2 p)
        {
            Triangulator triangulator = new Triangulator(this);
            int[] triangles = triangulator.Triangulate();

            bool pointInsideShape = false;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (Maths2D.PointInTriangle(points[triangles[i]], points[triangles[i + 1]], points[triangles[i + 2]], p))
                {
                    pointInsideShape = true;
                    break;
                }
            }

            return pointInsideShape;
        }

        public bool PointInsidePolygon(Polygon polygon, Vector2 p)
        {
            Triangulator triangulator = new Triangulator(polygon);
            int[] triangles = triangulator.Triangulate();

            bool pointInsideShape = false;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (Maths2D.PointInTriangle(polygon.points[triangles[i]], polygon.points[triangles[i + 1]], polygon.points[triangles[i + 2]], p))
                {
                    pointInsideShape = true;
                    break;
                }
            }

            return pointInsideShape;
        }

        public List<Vector2[]> GetVoronoiRegion(Vector3[] voronoiCenterPoints, float[] roomSizes, int voronoiIndex, out List<Vector2[]> splittingPoints, out List<bool[]> isCut)
        {
            Vector2[] voronoiCenterPoints2 = new Vector2[voronoiCenterPoints.Length];

            for (int i = 0; i < voronoiCenterPoints.Length; i++)
            {
                voronoiCenterPoints2[i] = new Vector2(voronoiCenterPoints[i].x, voronoiCenterPoints[i].z);
            }

            return GetVoronoiRegion(voronoiCenterPoints2, roomSizes, voronoiIndex, out splittingPoints, out isCut);
        }

        public List<Vector2[]> GetVoronoiRegion(Vector2[] voronoiCenterPoints, float[] roomSizes, int voronoiIndex, out List<Vector2[]> splittingPoints, out List<bool[]> isCut)
        {
            /*
            for (int i = 0; i < voronoiCenterPoints.Length; i++) 
            {
                //Debug.LogError("asdf TEST pos" + voronoiCenterPoints[i].ToString("F5"));
            }
            */

            //, out List<Vector2> seg1Points, out List<Vector2> seg2Points

            //Return Parameter
            List<Vector2[]> VoronoiRegionsSingle = new List<Vector2[]>();
            splittingPoints = new List<Vector2[]>();
            isCut = new List<bool[]>();

            //List of all Points
            List<Vector2> allPoints = new List<Vector2>(points);

            //Connectivity List
            //eg.: index 1 --> index 3, 5, 9
            List<List<int>> connectivity = new List<List<int>>();
            //Keep care of Holes
            int endindex;
            int startindex;

            for (int k = 0; k < numHoles + 1; k++)
            {
                //Startindex
                if (k == 0)
                {
                    startindex = 0;
                }
                else
                {
                    startindex = holeStartIndices[k - 1];
                }

                //Endindex
                if (numHoles > k)
                {
                    endindex = holeStartIndices[k] - 1;
                }
                else
                {
                    endindex = points.Length - 1;
                }


                for (int i = startindex; i <= endindex; i++)
                {
                    connectivity.Add(new List<int>());

                    if (i + 1 > endindex)
                    {
                        connectivity[startindex].Add(endindex);
                    }
                    else
                    {
                        connectivity[i].Add(i + 1);
                    }
                }
            }

            /*
            //Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            for (int i = 0; i < connectivity.Count; i++) 
            {
                for (int k = 0; k < connectivity[i].Count; k++) 
                {
                    //Debug.Log(i + " --> " + connectivity[i][k]);
                }
            }
            //Debug.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            */

            //Calculate connectivity hull
            List<List<int>> connectivityHull = new List<List<int>>();
            for (int i = 0; i < numHullPoints; i++)
            {
                connectivityHull.Add(new List<int>());
                if (i < numHullPoints - 1)
                {
                    connectivityHull[i].Add(i + 1);
                }
                if (i == 0 && numHullPoints > 2)
                {
                    connectivityHull[i].Add(numHullPoints - 1);
                }
            }

            /*
            //Calculate connectivity holes
            List<List<List<int>>> connectivityHoles = new List<List<List<int>>>();
            for (int k = 0; k < numHoles; k++)
            {
                startindex = holeStartIndices[k];

                //Endindex
                if (k < numHoles-1)
                {
                    endindex = holeStartIndices[k+1] - 1;
                }
                else
                {
                    endindex = points.Length - 1;
                }

                connectivityHoles.Add(new List<List<int>>());
                for (int i = startindex; i <= endindex; i++)
                {
                    connectivityHoles[k].Add(new List<int>());

                    if (i + 1 > endindex)
                    {
                        connectivityHoles[k][startindex].Add(endindex);
                    }
                    else
                    {
                        connectivityHoles[k][i].Add(i + 1);
                    }
                }
            }
            */


            //Calculate all intersection points to another room

            for (int i = 0; i < voronoiCenterPoints.Length; i++)
            {
                List<Vector2> intersectionPoints;
                List<Vector2> intersectionPointsSorted = new List<Vector2>();
                List<int> segPoint1Sorted = new List<int>();
                List<int> segPoint2Sorted = new List<int>();

                if (i != voronoiIndex)
                {
                    ////Debug.Log(voronoiCenterPoints.Length + " --> " + voronoiIndex + ", " + i);
                    Maths2D.CalculateCenterPerpendicular(new Vector2(voronoiCenterPoints[voronoiIndex].x, voronoiCenterPoints[voronoiIndex].y), new Vector2(voronoiCenterPoints[i].x, voronoiCenterPoints[i].y), roomSizes[voronoiIndex], roomSizes[i], out Vector2 middlepoint, out Vector2 direction);


                    List<int> segPoint1, segPoint2;
                    intersectionPoints = new List<Vector2>(GetIntersectionPoints(connectivity, allPoints, middlepoint, direction, out segPoint1, out segPoint2));

                    List<float> intersecSortingNum = new List<float>();
                    for (int k = 0; k < intersectionPoints.Count; k++)
                    {
                        float sortNum;
                        if (direction.x == 0)
                        {
                            sortNum = ((intersectionPoints[k].y - middlepoint.y) / direction.y);
                        }
                        else
                        {
                            sortNum = ((intersectionPoints[k].x - middlepoint.x) / direction.x);
                        }

                        intersecSortingNum.Add(sortNum);
                        ////Debug.Log("intersection sorted: " + intersecSortingNum[k]);
                    }

                    //Debug.LogError(intersectionPoints.Count);
                    int num = intersectionPoints.Count;
                    for (int k = 0; k < num; k++)
                    {
                        int minIndex = intersecSortingNum.IndexOf(intersecSortingNum.Min());
                        ////Debug.Log("intersection sorted: " + intersecSortingNum.Min());
                        intersectionPointsSorted.Add(intersectionPoints[minIndex]);
                        segPoint1Sorted.Add(segPoint1[minIndex]);
                        segPoint2Sorted.Add(segPoint2[minIndex]);

                        intersectionPoints.RemoveAt(minIndex);
                        intersecSortingNum.RemoveAt(minIndex);
                        segPoint1.RemoveAt(minIndex);
                        segPoint2.RemoveAt(minIndex);
                    }

                    ////Debug.LogWarning("#Intersections: " + intersectionPointsSorted.Count + " > " + voronoiCenterPoints[voronoiIndex] + " to " + voronoiCenterPoints[i] );




                    //TODO
                    //REWORK TO SUPPORT MORE HOLEFORMS

                    //NEW
                    List<int> fromHoleNum = new List<int>();
                    List<int> fromIndexNum = new List<int>();
                    List<int> toHoleNum = new List<int>();
                    List<int> toIndexNum = new List<int>();
                    ////Debug.LogWarning(intersectionPointsSorted.Count);

                    //Debug.LogError(intersectionPointsSorted.Count);

                    for (int k = 0; k < intersectionPointsSorted.Count; k++)
                    {
                        if (k > 0 && (intersectionPointsSorted[k] == intersectionPointsSorted[k - 1]))
                        {
                            ////Debug.LogWarning("-- " + k);
                            /*
                            //Take out the Corner, when the next Segment would be not inside the Object (= Outside or a Hole)
                            if (k + 1 >= intersectionPointsSorted.Count || !PointInsidePolygon((intersectionPointsSorted[k]+intersectionPointsSorted[k+1])/2))
                            {
                                //Punkt wird rausgenommen, sobald danach noch ein weiterer Punkt kommt 
                                //TODO WTF IST DAS: !PointInsidePolygon((intersectionPointsSorted[k]+intersectionPointsSorted[k+1])/2)
                                //TODO FULL REWORK --> JUST CHECK IF EVERY SEGMENT IS INSIDE THE POLYGON, IF NOT DISCARD IT, IF YES KEEP IT
                                //DAHER ZUERST SCHAUEN OB AN BEIDEN ENDEN SCHON DER SPLIT EINGFÜGT WURDE, FALLS JA (DER INTERSECTION POINT BEREITS EIN ECKPUNKT IST) BRAUCHT MAN NICHTS TUN, FALLS NEIN
                                //SEGMENT SPLITTEN UND NEUEN ECKPUNKT EINFÜGEN (DABEI EVTL: DARAUF ACHTEN ZU MERKEN, WELCHE PUNKTE RAND/HOLE PUNKTE WAREN), DANACH SCHAUEN, OB DAS NEU HINZUZUFÜGENDE ELEMENT
                                //IM POLYGON LIEGT --> MITTELPUNKT DES SEGMENTS BERECHNEN & POINTINSIDEPOLYGON() NUTZEN
                                //FALLS JA, SEGMENT DER MENGE DER MÖGLICHEN SEGMENTE FÜR DIESEN RAUM HINZUFÜGEN
                                //PROBLEM: gebogene Holes, bei denen der Innere Teil komplett zu einem Raum gehört, auch wenn die Trennlinie eigentlich hindurch verläuft
                                intersectionPointsSorted.RemoveAt(k);
                                segPoint1Sorted.RemoveAt(k);
                                segPoint2Sorted.RemoveAt(k);
                            }
                            //Debug.LogWarning("Splitting through a Corner! --> Maybe more steps needed!");
                            */
                            //Take duplicate entries out in every situation
                            intersectionPointsSorted.RemoveAt(k);
                            segPoint1Sorted.RemoveAt(k);
                            segPoint2Sorted.RemoveAt(k);
                            k--;
                        }
                        else
                        {
                            int holeNumber = -1;
                            for (int m = 0; m < numHoles; m++)
                            {
                                startindex = holeStartIndices[m];
                                if (m < numHoles - 1)
                                {
                                    endindex = holeStartIndices[m + 1] - 1;
                                }
                                else
                                {
                                    endindex = points.Length - 1;
                                }
                                if (Maths2D.IsBetween(segPoint1Sorted[k], startindex, endindex))
                                {
                                    holeNumber = m;
                                }
                            }

                            if (k % 2 == 0)
                            {
                                //The new Connection starting in this point
                                fromHoleNum.Add(holeNumber);
                                fromIndexNum.Add(k);
                            }
                            else
                            {
                                //The new Connection ending in this point
                                toHoleNum.Add(holeNumber);
                                toIndexNum.Add(k);
                            }
                            //Debug.LogError(k + " --> hole: " + holeNumber + " (fromCount= " + fromHoleNum.Count + ", toCount = " + toHoleNum.Count + ") -->" + intersectionPointsSorted[k] + " count: " + intersectionPointsSorted.Count);
                        }
                    }

                    //calculate possible splittings
                    List<List<int>> possibleSplittingIndexes = new List<List<int>>();
                    List<List<bool>> addConnectivity = new List<List<bool>>();

                    int save = 0;
                    while (save < 500 && fromHoleNum.Count > 0) 
                    {
                        possibleSplittingIndexes.Add(new List<int>());
                        addConnectivity.Add(new List<bool>());

                        /*
                        if (fromHoleNum.Count == 0 || toHoleNum.Count == 0) 
                        {
                            //Debug.LogError("COUNT OF SPLITTINGS == 0  (from: " + fromHoleNum.Count + ", to: " + toHoleNum.Count + ")");
                        }
                        */
                        //Debug.LogError(i  + " asdf TEST " + fromHoleNum.Count + " to " + toHoleNum.Count);
                        int startHoleNum = fromHoleNum[0];
                        int endHoleNum = toHoleNum[0];
                        

                        possibleSplittingIndexes[possibleSplittingIndexes.Count - 1].Add(fromIndexNum[0]);
                        possibleSplittingIndexes[possibleSplittingIndexes.Count - 1].Add(toIndexNum[0]);
                        addConnectivity[addConnectivity.Count - 1].Add(true);

                        fromHoleNum.RemoveAt(0);
                        toHoleNum.RemoveAt(0);
                        fromIndexNum.RemoveAt(0);
                        toIndexNum.RemoveAt(0);

                        int index = 0;
                        int save2 = 0;
                        while (save2 < 500 && startHoleNum != endHoleNum) 
                        {
                            for (int k = fromHoleNum.Count - 1; k >= index; k--) 
                            {
                                if (fromHoleNum[k] == endHoleNum) 
                                {
                                    endHoleNum = toHoleNum[k];

                                    addConnectivity[addConnectivity.Count - 1].Add(false);
                                    possibleSplittingIndexes[possibleSplittingIndexes.Count - 1].Add(fromIndexNum[k]);
                                    possibleSplittingIndexes[possibleSplittingIndexes.Count - 1].Add(toIndexNum[k]);
                                    addConnectivity[addConnectivity.Count - 1].Add(true);

                                    fromHoleNum.RemoveAt(k);
                                    toHoleNum.RemoveAt(k);
                                    fromIndexNum.RemoveAt(k);
                                    toIndexNum.RemoveAt(k);

                                    index = k;
                                }
                            }
                            save2++;
                        }
                        save++;
                    }


                    //ENDE DER ENTFERNUNG NICHT GÜLTIGER TRENNUNGEN
                    
                    int indexOfPossibleSplittings = possibleSplittingIndexes.Count-1;

                    ////Debug.LogWarning("num: " + possibleSplittingIndexes.Count);

                    int lastSegmentIndexFrom = -1;
                    int lastSegmentIndexTo = -1;

                    for (int k = 0; k < indexOfPossibleSplittings + 1; k++)
                    {
                        //Create a working copy of the connectivity matrix and the points list
                        List<List<int>> connectivity2 = new List<List<int>>();
                        for (int a = 0; a < connectivity.Count; a++)
                        {
                            connectivity2.Add(new List<int>());
                            for (int b = 0; b < connectivity[a].Count; b++)
                            {

                                connectivity2[a].Add(connectivity[a][b]);
                            }
                        }
                        List<Vector2> allPoints2 = new List<Vector2>(allPoints.ToArray());


                        for (int m = 0; m < possibleSplittingIndexes[k].Count; m+=2)
                        {
                            //Adding the intersections + split up already existing connections, adding new connections between the intersections
                            for (int l = 0; l < 2; l++)
                            {
                                int index = possibleSplittingIndexes[k][m+l];
                                allPoints2.Add(intersectionPointsSorted[index]);
                                connectivity2.Add(new List<int>());
                                int indexOfIntersectionPoint = allPoints2.Count - 1;
                                RemoveConnectivity(connectivity2, segPoint1Sorted[index], segPoint2Sorted[index]);
                                connectivity2[segPoint1Sorted[index]].Add(indexOfIntersectionPoint);
                                connectivity2[indexOfIntersectionPoint].Add(segPoint2Sorted[index]);
                            }
                            connectivity2[allPoints2.Count - 2].Add(allPoints2.Count - 1);

                            lastSegmentIndexFrom = allPoints2.Count - 2;
                            lastSegmentIndexTo = allPoints2.Count - 1;
                        }

                        List<Vector2> hullPointsNew = new List<Vector2>();
                        int nextIndex = lastSegmentIndexTo;
                        int targetIndex = lastSegmentIndexFrom;
                        int lastIndex = lastSegmentIndexFrom;

                        ////Debug.LogWarning(nextIndex + " (" + allPoints2[nextIndex] + "), " + targetIndex + " (" + allPoints2[targetIndex] + ")");

                        int save3 = 0;
                        while (nextIndex != targetIndex && save3 < 100)
                        {
                            ////Debug.Log("counter: " + save);
                            if (!Maths2D.ListContainsVector(hullPointsNew, allPoints2[nextIndex]))
                            {
                                ////Debug.Log("---------------------");
                                hullPointsNew.Add(allPoints2[nextIndex]);
                            }
                            List<int> nextPossibleIndecies = GetConnectivityForward(connectivity2, nextIndex, lastIndex);

                            ////Debug.Log("current: " + nextIndex + ", last: " + lastIndex + " --> " + nextPossibleIndecies[0] + " or " + nextPossibleIndecies[1]);
                            /*
                            for (int test = 0; test < nextPossibleIndecies.Count; test++) 
                            {
                                //Debug.LogWarning(">>>: " + nextPossibleIndecies[test]);
                            }
                            */

                            int newIndex;
                            if (nextPossibleIndecies.Count == 1)
                            {
                                newIndex = nextPossibleIndecies[0];
                            }
                            else
                            {
                                ////Debug.Log("ADDITIONAL: " + save);
                                newIndex = GetNextIndex(allPoints2, nextIndex, lastIndex, nextPossibleIndecies, 0, 0.01f);
                                ////Debug.Log(newIndex);
                            }
                            ////Debug.LogWarning(">>>: " + newIndex + " (" + allPoints2[newIndex] + ")" + " :<<<");
                            //hullPointsNew.Add(allPoints2[nextIndex]);
                            //////hullPointsNew.Add(allPoints2[newIndex]);
                            lastIndex = nextIndex;
                            nextIndex = newIndex;
                            save3++;
                            ////Debug.Log("ADD: " + nextIndex + " --> " + allPoints2[nextIndex]);
                        }
                        if (!Maths2D.ListContainsVector(hullPointsNew, allPoints2[nextIndex])) 
                        {
                            hullPointsNew.Add(allPoints2[nextIndex]);
                        }

                        

                        /*
                        if (hullPointsNew.Count <= 3) 
                        {
                            //Debug.LogError("to small number of hullpoints (" + hullPointsNew.Count + ")");         
                        }
                        */

                        Polygon p = new Polygon(hullPointsNew.ToArray());
                        Triangulator t = new Triangulator(p);
                        int[] triangles = t.Triangulate();
                        ////Debug.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                        if ((p.PointInsidePolygon(voronoiCenterPoints[i]) && !p.PointInsidePolygon(voronoiCenterPoints[voronoiIndex])) || (!p.PointInsidePolygon(voronoiCenterPoints[i]) && p.PointInsidePolygon(voronoiCenterPoints[voronoiIndex])))
                        {
                            ////Debug.Log("YES");
                            //TODO ADDING HOLES BACK IN THE POLYGON
                            //return hullPointsNew.ToArray();
                            VoronoiRegionsSingle.Add(hullPointsNew.ToArray());
                            isCut.Add(addConnectivity[k].ToArray());

                            //Vector2[] splittingPointsSingle = new Vector2[possibleSplittingIndexes[k].Count];
                            Vector2[] splittingPointsSingle = new Vector2[possibleSplittingIndexes[k].Count];
                            for (int m = 0; m < possibleSplittingIndexes[k].Count; m++)
                            {
                                int index = possibleSplittingIndexes[k][m];
                                splittingPointsSingle[m] = intersectionPointsSorted[index];
                            }
                            splittingPoints.Add(splittingPointsSingle);
                        }
                        else
                        {
                            ////Debug.Log("NO");
                            //return hullPointsNew.ToArray();
                        }

                    }


                }

                ////Debug.Log("--------------------voronoi------------------------------");
            }


            //Sorting all intersection points
            return VoronoiRegionsSingle;
        }

        private bool ContainsVector(List<Vector2> list, Vector2 subject) 
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].x == subject.x && list[i].y == subject.y)
                {
                    return true;
                }
            }
            return false;
        }

        private int IndexOfVector(List<Vector2> list, Vector2 subject)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].x == subject.x && list[i].y == subject.y)
                {
                    return i;
                }
            }
            return -1;
        }

        public Vector2[] GetSingleVoronoiCell(List<Vector2[]> splittingPoints, List<bool[]> isCut, Vector2 voronoiCenterPoint, out List<List<int>> connectivity, out List<Vector2> allPoints)
        {
            //List<Vector2> allPoints = new List<Vector2>(points);
            allPoints = new List<Vector2>(points);

            //Connectivity List
            //eg.: index 1 --> index 3, 5, 9
            //List<List<int>> connectivity = new List<List<int>>();
            connectivity = new List<List<int>>();

            /*
            for (int i = 0; i <= IndexOfFirstPointInHole(0)-1; i++)
            {
                connectivity.Add(new List<int>());

                if (i + 1 > IndexOfFirstPointInHole(0)-1)
                {
                    connectivity[0].Add(IndexOfFirstPointInHole(0)-1);
                }
                else
                {
                    connectivity[i].Add(i + 1);
                }
            }*/


            int endindex;
            int startindex;

            for (int k = 0; k < numHoles + 1; k++)
            {
                //Startindex
                if (k == 0)
                {
                    startindex = 0;
                }
                else
                {
                    startindex = holeStartIndices[k - 1];
                }

                //Endindex
                if (numHoles > k)
                {
                    endindex = holeStartIndices[k] - 1;
                }
                else
                {
                    endindex = points.Length - 1;
                }


                for (int i = startindex; i <= endindex; i++)
                {
                    connectivity.Add(new List<int>());

                    if (i + 1 > endindex)
                    {
                        connectivity[startindex].Add(endindex);
                    }
                    else
                    {
                        connectivity[i].Add(i + 1);
                    }
                }
            }

            List<int> indicesCut;

            for (int i = 0; i < splittingPoints.Count; i++)
            {
                for (int k = 0; k < splittingPoints[i].Length; k++)
                {
                    //if (k == splittingPoints[i].Length-1 || isCut[i][k] || (k >= 1 && isCut[i][k - 1]))
                    if ((k >= 1 && isCut[i][k - 1]) || isCut[i][k])
                    {
                        //ADD Point
                        allPoints.Add(splittingPoints[i][k]);
                        connectivity.Add(new List<int>());

                        //Iterate over all segments and search for the segment that get Cutted, then cut it
                        indicesCut = new List<int>();
                        for (int m = 0; m < connectivity.Count; m++)
                        {
                            for (int n = 0; n < connectivity[m].Count; n++)
                            {
                                if (Maths2D.PointOnLineSegment(allPoints[m], allPoints[connectivity[m][n]], splittingPoints[i][k]))
                                {
                                    indicesCut.Add(m);
                                    indicesCut.Add(connectivity[m][n]);
                                }
                            }
                        }
                        for (int m = 0; m < indicesCut.Count; m += 2)
                        {
                            //RemoveConnectivity(connectivity, indicesCut[m], indicesCut[m + 1]);
                            //connectivity[indicesCut[m]].Add(allPoints.Count - 1);
                            //connectivity[indicesCut[m + 1]].Add(allPoints.Count - 1);
                            SplitConnectivity(connectivity, indicesCut[m], indicesCut[m + 1], allPoints.Count - 1);
                        }

                    }
                    if (k < splittingPoints[i].Length - 1 && isCut[i][k])
                    {
                        //ADD Segment 
                        connectivity[connectivity.Count - 1].Add(connectivity.Count);
                    }
                }
            }


            //Calculate new Cutting Points
            List<Vector2> newCuttingPoints = new List<Vector2>();

            for (int i = 0; i < splittingPoints.Count; i++)
            {
                for (int k = 0; k < splittingPoints[i].Length - 1; k++)
                {
                    if (isCut[i][k])
                    {
                        //Test Segment
                        //splittingPoints[i][k] to splittingPoints[i][k+1]
                        for (int m = i; m < splittingPoints.Count; m++)
                        {
                            for (int n = 0; n < splittingPoints[m].Length - 1; n++)
                            {
                                if (isCut[m][n])
                                {
                                    //Test Segment
                                    //splittingPoints[m][n] to splittingPoints[m][n+1]
                                    //if (Maths2D.AreLinesIntersecting(splittingPoints[i][k], splittingPoints[i][k + 1], splittingPoints[m][n], splittingPoints[m][n + 1], false, out Vector2 intersection)) 
                                    if (Maths2D.LineSegmentsIntersect(splittingPoints[i][k], splittingPoints[i][k + 1], splittingPoints[m][n], splittingPoints[m][n + 1], out Vector2 intersection))
                                    {
                                        newCuttingPoints.Add(intersection);
                                    }

                                }
                            }
                        }

                    }
                }
            }

            ////Debug.LogWarning("TEST: " + newCuttingPoints.Count);
            //ADD new Cutting Points + split up connections

            for (int i = 0; i < newCuttingPoints.Count; i++)
            {
                indicesCut = new List<int>();
                allPoints.Add(newCuttingPoints[i]);
                connectivity.Add(new List<int>());

                for (int m = 0; m < connectivity.Count; m++)
                {
                    for (int n = 0; n < connectivity[m].Count; n++)
                    {
                        if (Maths2D.PointOnLineSegment(allPoints[m], allPoints[connectivity[m][n]], newCuttingPoints[i]))
                        {
                            indicesCut.Add(m);
                            indicesCut.Add(connectivity[m][n]);
                        }
                    }
                }
                for (int m = 0; m < indicesCut.Count; m += 2)
                {
                    //RemoveConnectivity(connectivity, indicesCut[m], indicesCut[m + 1]);
                    //connectivity[indicesCut[m]].Add(allPoints.Count - 1);
                    //connectivity[indicesCut[m + 1]].Add(allPoints.Count - 1);
                    SplitConnectivity(connectivity, indicesCut[m], indicesCut[m + 1], allPoints.Count - 1);
                }
            }

            //GET 1 SEGMENT THAT BELONGS SAVE TO THE ROOM
            List<int> segPoint1Unsorted, segPoint2Unsorted;
            List<Vector2> intersectionPointsUnsorted = new List<Vector2>(GetIntersectionPoints(connectivity, allPoints, voronoiCenterPoint, Vector2.right, out segPoint1Unsorted, out segPoint2Unsorted));
            float smallestNumAboveZero = -1;
            int smallestSegP1 = -1;
            int smallestSegP2 = -1;

            List<float> intersecSortingNum = new List<float>();
            for (int k = 0; k < intersectionPointsUnsorted.Count; k++)
            {
                ////Debug.LogError(k + " P1: " + allPoints[smallestSegP1]);
                ////Debug.LogError(k + " P2: " + allPoints[smallestSegP2]);
                ////Debug.LogError(k + " intersect: " + intersectionPoints[k]);
                float num;
                if (Vector2.right.x == 0)
                {
                    num = ((intersectionPointsUnsorted[k].y - voronoiCenterPoint.y) / Vector2.right.y);
                }
                else
                {
                    num = ((intersectionPointsUnsorted[k].x - voronoiCenterPoint.x) / Vector2.right.x);
                }

                intersecSortingNum.Add(num);
                if (smallestNumAboveZero < 0 && num >= 0)
                {
                    smallestNumAboveZero = num;
                    smallestSegP1 = segPoint1Unsorted[k];
                    smallestSegP2 = segPoint2Unsorted[k];
                }
                else if (num >= 0 && num <= smallestNumAboveZero)
                {
                    smallestNumAboveZero = num;
                    smallestSegP1 = segPoint1Unsorted[k];
                    smallestSegP2 = segPoint2Unsorted[k];
                }
            }

            //PROBLEM HOLE
            List<Vector2> intersectionPoints = new List<Vector2>();
            List<int> segPoint1 = new List<int>();
            List<int> segPoint2 = new List<int>();

            int intersections = intersectionPointsUnsorted.Count;
            for (int k = 0; k < intersections; k++)
            {
                float min = intersecSortingNum.Min();
                int minIndex = intersecSortingNum.IndexOf(min);
                ////Debug.Log("intersection sorted: " + intersecSortingNum.Min());
                if (min >= 0)
                {
                    intersectionPoints.Add(intersectionPointsUnsorted[minIndex]);
                    segPoint1.Add(segPoint1Unsorted[minIndex]);
                    segPoint2.Add(segPoint2Unsorted[minIndex]);
                }
                intersectionPointsUnsorted.RemoveAt(minIndex);
                intersecSortingNum.RemoveAt(minIndex);
                segPoint1Unsorted.RemoveAt(minIndex);
                segPoint2Unsorted.RemoveAt(minIndex);
            }


            bool possibleSegmentFound = false;
            int seg1 = -1;
            int seg2 = -1;

            ////Debug.LogError(segPoint1.Count + ", " + segPoint2.Count);

            int s = 0;
            while (!possibleSegmentFound && s < 1000)
            {
                
                seg1 = segPoint1[0];
                seg2 = segPoint2[0];

                segPoint1.RemoveAt(0);
                segPoint2.RemoveAt(0);

                int holeIndex1 = -1;
                int holeIndex2 = -1;
                if (numHoles > 0 && (seg1 >= holeStartIndices[0] && seg1 < numPoints) && (seg2 >= holeStartIndices[0] && seg2 < numPoints))
                {

                    for (int i = 0; i < numHoles - 1; i++)
                    {
                        if (seg1 >= holeStartIndices[i] && seg1 < holeStartIndices[i + 1])
                        {
                            holeIndex1 = i;
                        }

                        if (seg2 >= holeStartIndices[i] && seg2 < holeStartIndices[i + 1])
                        {
                            holeIndex2 = i;
                        }
                    }
                    if (seg1 >= holeStartIndices[numHoles - 1] && seg1 < numPoints)
                    {
                        holeIndex1 = numHoles - 1;
                    }

                    if (seg2 >= holeStartIndices[numHoles - 1] && seg2 < numPoints)
                    {
                        holeIndex2 = numHoles - 1;
                    }

                    ////Debug.LogError("hole1: " + holeIndex1 + ", hole2: " + holeIndex2 + " --> numHoles: " + numHoles);

                }

                possibleSegmentFound = true;

                if (holeIndex1 == holeIndex2 && holeIndex1 != -1)
                {
                    //Is Hole complete inside room?

                    int start = holeStartIndices[holeIndex1];
                    int end;
                    if (holeIndex1 < numHoles - 1)
                    {
                        end = holeStartIndices[holeIndex1 + 1];
                    }
                    else
                    {
                        end = numPoints;
                    }

                    bool allInside = true;
                    for (int i = start; i < end - 1; i++)
                    {
                        if (!connectivity[i].Contains((i + 1)))
                        {
                            allInside = false;
                        }
                    }
                    if (!connectivity[start].Contains((end - 1)))
                    {
                        allInside = false;
                    }

                    if (allInside)
                    {
                        possibleSegmentFound = false;
                        //if complete inside
                        segPoint1.RemoveAt(0);
                        segPoint2.RemoveAt(0);
                    }
                    ////Debug.LogError(allInside);
                }
                s++;
            }

            ////Debug.LogError(possibleSegmentFound + ", " + seg1 + "; " + seg2);


            //int nextIndex = smallestSegP2;
            //int targetIndex = smallestSegP1;

            int nextIndex = seg1;
            int targetIndex = seg2;

            ////Debug.LogError("target: " + allPoints[smallestSegP2]);
            ////Debug.LogError("last: " + allPoints[smallestSegP1]);

            List<Vector2> hullPointsNew = new List<Vector2>();

            //int lastIndex = smallestSegP1;
            int lastIndex = seg2;

            int save = 0;
            while (nextIndex != targetIndex && save < 100)
            {
                //ERROR nextIndex = -1
                ////Debug.LogError(">>>" + nextIndex + "; " + lastIndex);
                hullPointsNew.Add(allPoints[nextIndex]);
                List<int> nextPossibleIndecies = GetConnectivityForward(connectivity, nextIndex, lastIndex);
                ////Debug.Log("current: " + nextIndex + ", last: " + lastIndex + " --> " + nextPossibleIndecies[0] + " or " + nextPossibleIndecies[1]);
                int newIndex;
                if (nextPossibleIndecies.Count == 1)
                {
                    newIndex = nextPossibleIndecies[0];
                }
                else
                {
                    newIndex = GetNextIndex(allPoints, nextIndex, lastIndex, nextPossibleIndecies, save, 0.01f);
                }
                ////Debug.LogError(nextPossibleIndecies.Count + ", " + newIndex);
                lastIndex = nextIndex;
                nextIndex = newIndex;
                save++;
            }
            hullPointsNew.Add(allPoints[nextIndex]);

            Polygon p = new Polygon(hullPointsNew.ToArray());

            if (!p.PointInsidePolygon(voronoiCenterPoint))
            {
                nextIndex = seg2;
                targetIndex = seg1;
                hullPointsNew = new List<Vector2>();
                lastIndex = seg1;

                save = 0;
                while (nextIndex != targetIndex && save < 100)
                {
                    hullPointsNew.Add(allPoints[nextIndex]);
                    List<int> nextPossibleIndecies = GetConnectivityForward(connectivity, nextIndex, lastIndex);
                    ////Debug.Log("current: " + nextIndex + ", last: " + lastIndex + " --> " + nextPossibleIndecies[0] + " or " + nextPossibleIndecies[1]);
                    int newIndex;
                    if (nextPossibleIndecies.Count == 1)
                    {
                        newIndex = nextPossibleIndecies[0];
                    }
                    else
                    {
                        newIndex = GetNextIndex(allPoints, nextIndex, lastIndex, nextPossibleIndecies, save, 0.01f);
                    }
                    lastIndex = nextIndex;
                    nextIndex = newIndex;
                    save++;
                }
                hullPointsNew.Add(allPoints[nextIndex]);
            }

            return hullPointsNew.ToArray();
        }

        /*
        private List<Vector2> GetNewHullPoints() 
        {
            int nextIndex = lastSegmentIndexTo;
            int targetIndex = lastSegmentIndexFrom;
            ////Debug.Log("next: " + nextIndex + ", target: " + targetIndex);
            List<Vector2> hullPointsNew = new List<Vector2>();
            ////Debug.Log("AllPointsSize: " + allPoints2.Count + " aber nächster Index: " + lastSegmentIndexTo + " und letzter: " + lastSegmentIndexFrom);
            //hullPointsNew.Add(allPoints2[lastSegmentIndexFrom]);
            //hullPointsNew.Add(allPoints2[nextIndex]);
            int lastIndex = lastSegmentIndexFrom;
            ////Debug.Log("ADD: " + nextIndex + " --> " + allPoints2[nextIndex]);

            int save = 0;
            while (nextIndex != targetIndex && save < 100)
            {
                ////Debug.Log("counter: " + save);
                hullPointsNew.Add(allPoints2[nextIndex]);
                List<int> nextPossibleIndecies = GetConnectivityForward(connectivity2, nextIndex, lastIndex);
                ////Debug.Log("current: " + nextIndex + ", last: " + lastIndex + " --> " + nextPossibleIndecies[0] + " or " + nextPossibleIndecies[1]);
                int newIndex;
                if (nextPossibleIndecies.Count == 1)
                {
                    newIndex = nextPossibleIndecies[0];
                }
                else
                {
                    ////Debug.Log("ADDITIONAL: " + save);
                    newIndex = GetNextIndex(allPoints2, nextIndex, lastIndex, nextPossibleIndecies);
                    ////Debug.Log(newIndex);
                }
                //hullPointsNew.Add(allPoints2[nextIndex]);
                //////hullPointsNew.Add(allPoints2[newIndex]);
                lastIndex = nextIndex;
                nextIndex = newIndex;
                save++;
                ////Debug.Log("ADD: " + nextIndex + " --> " + allPoints2[nextIndex]);
            }
            ////Debug.Log("nextIndex: " + nextIndex + " all poitns size: " + allPoints2.Count);
            hullPointsNew.Add(allPoints2[nextIndex]);
        }
        */
        private void RemoveConnectivity(List<List<int>> connectivityMat, int from, int to)
        {
            connectivityMat[from].Remove(to);
            connectivityMat[to].Remove(from);
        }

        private void SplitConnectivity(List<List<int>> connectivityMat, int from, int to, int split)
        {
            if (connectivityMat[from].Remove(to)) 
            {
                connectivityMat[from].Add(split);
                connectivityMat[split].Add(to);
            }
            if (connectivityMat[to].Remove(from)) 
            {
                connectivityMat[to].Add(split);
                connectivityMat[split].Add(from);
            }
        }

        private List<int> GetConnectivity(List<List<int>> connectivityMat, int from) 
        {
            List<int> connections = new List<int>();
            for (int i = 0; i < connectivityMat.Count; i++) 
            {
                if (i == from)
                {
                    foreach (int content in connectivityMat[i])
                    {
                        if (!connections.Contains(content))
                        {
                            connections.Add(content);
                        }
                    }
                }
                else 
                {
                    for (int k = 0; k < connectivityMat[i].Count; k++)
                    {
                        if (connectivityMat[i][k] == from) 
                        {
                            if (!connections.Contains(i))
                            {
                                connections.Add(i);
                            }
                        }
                    }
                }
            }
            return connections;
        }

        private List<int> GetConnectivityForward(List<List<int>> connectivityMat, int from, int last)
        {
            List<int> connections = GetConnectivity(connectivityMat, from);
            ////Debug.LogWarning(connections.Count);
            connections.Remove(last);
            
            return connections;
        }

        private int GetNextIndex(List<Vector2> allP, int currentIndex, int lastIndex, List<int> nextPossibleIndieces, int test, float epsilon) 
        {
            Vector2 compDirection = allP[lastIndex]-allP[currentIndex];
            //Vector2 compDirection = allP[currentIndex] - allP[lastIndex];
            float angle = 361;
            int nextIndex = -1;

            int i = 0;
            foreach (int index in nextPossibleIndieces) 
            {
                
                Vector2 nextDirection = allP[index] - allP[currentIndex];
                float signedAngle = (Vector2.SignedAngle(compDirection, nextDirection) +360)%360;
                if (signedAngle <= epsilon)
                {
                    signedAngle = 360;
                }

                if (signedAngle < angle) 
                {
                    angle = signedAngle;
                    nextIndex = index;
                }
                if (test == 5) 
                {
                    ////Debug.LogWarning("winkel: " + signedAngle);
                    ////Debug.LogWarning("next INDEX " + i + ": " + allP[index].x + ", " + allP[index].y);
                }
                i++;
            }
            if (test == 5)
            {
                ////Debug.LogWarning("---");
                ////Debug.LogWarning("LastINDEX: " + allP[lastIndex].x + ", " +  allP[lastIndex].y);
                ////Debug.LogWarning("CurrentINDEX: " + allP[currentIndex].x + ", " + allP[currentIndex].y);
            }
            ////Debug.LogWarning(nextIndex);
            if (nextIndex == -1) 
            {
                //Debug.LogError("Wrong Index! <-- should not happen");
            }
            return nextIndex;
        }


        public Vector2[] GetIntersectionPoints(List<List<int>> connectivityMat, List<Vector2> allpoints, Vector2 point, Vector2 directionVec, out List<int> indexSegPoint1, out List<int> indexSegPoint2)
        {
            List<Vector2> intersectionPoints = new List<Vector2>();
            indexSegPoint1 = new List<int>();
            indexSegPoint2 = new List<int>();

            for (int i = 0; i < connectivityMat.Count; i++)
            {
                for (int k = 0; k < connectivityMat[i].Count; k++)
                {
                    Vector2 intersection;
                    bool intersect;
                    intersect = Maths2D.LineLineSegmentIntersection(out intersection, point, directionVec, allpoints[i], allpoints[connectivityMat[i][k]]);


                    if (intersect)
                    {
                        intersectionPoints.Add(intersection);
                        indexSegPoint1.Add(i);
                        indexSegPoint2.Add(connectivityMat[i][k]);
                    }
                }
            }
            return intersectionPoints.ToArray();
        }

        public Vector2[] GetIntersectionPoints(Vector2 point, Vector2 directionVec) 
        {
            List<Vector2> intersectionPoints = new List<Vector2>();

            //Keep care of Holes
            int endindex = points.Length - 1;
            int startindex = 0;

            for (int k = 0; k < numHoles + 1; k++)
            {
                //Startindex
                if (k == 0)
                {
                    startindex = 0;
                }
                else
                {
                    startindex = holeStartIndices[k-1];
                }

                //Endindex
                if (numHoles > k)
                {
                    endindex = holeStartIndices[k] - 1;
                }
                else 
                {
                    endindex = points.Length - 1;
                }


                for (int i = startindex; i <= endindex; i++)
                {
                    Vector2 intersection;
                    bool intersect;

                    if (i + 1 > endindex)
                    {
                        intersect = Maths2D.LineLineSegmentIntersection(out intersection, point, directionVec, points[i], points[startindex]);
                    }
                    else
                    {
                        intersect = Maths2D.LineLineSegmentIntersection(out intersection, point, directionVec, points[i], points[i + 1]);
                    }

                    if (intersect)
                    {
                        intersectionPoints.Add(intersection);
                    }

                }


                    
            }   
            return intersectionPoints.ToArray();
        }

    }

}