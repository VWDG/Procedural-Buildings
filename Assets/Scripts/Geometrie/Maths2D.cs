using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public static class Maths2D
    {
        public static float PseudoDistanceFromPointToLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return Mathf.Abs((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
        }

        public static int SideOfLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return (int)Mathf.Sign((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
        }

        public static int SideOfLine(float ax, float ay, float bx, float by, float cx, float cy)
        {
            return (int)Mathf.Sign((cx - ax) * (-by + ay) + (cy - ay) * (bx - ax));
        }

        public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0 && t >= 0 && (s + t) <= 1;

        }

        public static bool PointInTriangleExclusive(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s > 0 && t > 0 && (s + t) < 1;

        }

        public static bool LineSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            if (Mathf.Approximately(denominator, 0))
            {
                return false;
            }

            float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

            if (Mathf.Approximately(numerator1, 0) || Mathf.Approximately(numerator2, 0))
            {
                return false;
            }

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r > 0 && r < 1) && (s > 0 && s < 1);
        }

        public static bool LineSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            if (Mathf.Approximately(denominator, 0))
            {
                return false;
            }

            float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

            if (Mathf.Approximately(numerator1, 0) || Mathf.Approximately(numerator2, 0))
            {
                return false;
            }

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;
            bool intersecting = ((r > 0 && r < 1) && (s > 0 && s < 1));

            intersection = new Vector2((a.x + r * (b.x - a.x)), (a.y + r * (b.y - a.y)));

            return intersecting;
        }

        public static bool CalculatePerpendicularToSegmentThroughPoint(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, out Vector2 pointOnSegment) 
        {
            Vector2 perpendicular = Vector2.Perpendicular(segmentEnd - segmentStart);
            //Debug.Log(">>>( " + segmentStart + " - " + (segmentEnd - segmentStart).normalized + " - " + point + " - " + perpendicular.normalized + ")<<<");
            LineLineIntersection(out Vector2 intersection, segmentStart, (segmentEnd - segmentStart).normalized, point, perpendicular.normalized);
            pointOnSegment = intersection;

            if (PointOnLineSegment(segmentStart, segmentEnd, intersection)) 
            { 
                return true;
            }

            return false;
        }

        public static float CalculateAngleBetweenTwoVectors(Vector2 segment1, Vector2 connectedPoint, Vector2 segment2) 
        {
            Vector2 a = (segment1 - connectedPoint).normalized;
            Vector2 b = (segment2 - connectedPoint).normalized;

            return Vector2.Angle(a, b);
        }

        public static bool IsAngleBetweenTwoVectors180or360Degree(Vector2 segment1, Vector2 connectedPoint, Vector2 segment2, float culinaryArea) 
        {
            float angle = CalculateAngleBetweenTwoVectors(segment1, connectedPoint, segment2);
            if ((angle < 180 + culinaryArea && angle > 180 - culinaryArea) || (angle < 0 + culinaryArea && angle > 0 - culinaryArea)) 
            {
                return true;
            }
            return false;
        }

        public static bool IsAngleBetweenTwoVectors180or360Degree(Vector2 segment1, Vector2 segment2, float culinaryArea)
        {
            float angle = Vector2.Angle(segment1.normalized, segment2.normalized);
            if ((angle - culinaryArea < 180 && angle + culinaryArea > 180) || (angle - culinaryArea < 0 && angle + culinaryArea > 0))
            {
                return true;
            }
            return false;
        }

        public static bool IsAngleBetweenTwoVectors180or360Degree(float angle, float culinaryArea)
        {
            angle = Mathf.Abs(angle);
            if ((angle - culinaryArea < 180 && angle + culinaryArea > 180) || (angle - culinaryArea < 0 && angle + culinaryArea > 0))
            {
                return true;
            }
            return false;
        }

        public static bool IsAngleBetweenTwoVectors180Degree(Vector2 segment1, Vector2 connectedPoint, Vector2 segment2, float culinaryArea)
        {
            float angle = CalculateAngleBetweenTwoVectors(segment1, connectedPoint, segment2);
            if ((angle < 180 + culinaryArea && angle > 180 - culinaryArea))
            {
                return true;
            }
            return false;
        }

        public static bool IsAngleBetweenTwoVectors90Degree(Vector2 segment1, Vector2 segment2, float culinaryArea)
        {
            if (Mathf.Abs(Vector2.Dot(segment1, segment2)) < culinaryArea)
            {
                //Debug.LogError("Winkel: " + Vector2.Angle(segment1, segment2));
                return true;
            }
            return false;
        }


        public static Vector2 CalculateMiddlePoint(Vector2 a, Vector2 b, float weightA, float weightB) 
        {
            float fac = weightA / (weightA + weightB);

            //return new Vector2(a.x + 0.5f * (b.x - a.x), a.y + 0.5f * (b.y - a.y));
            return new Vector2(a.x + fac * (b.x - a.x), a.y + fac * (b.y - a.y));
        }

        public static void CalculateCenterPerpendicular(Vector2 a, Vector2 b, float weightA, float weightB, out Vector2 middlePoint, out Vector2 direction) 
        {
            Vector3 distanceVec = new Vector3(b.x - a.x, 0, b.y - a.y);

            Vector3 directionVec = Vector3.Cross(distanceVec, Vector3.up);
            direction = new Vector2(directionVec.x, directionVec.z);
            middlePoint = CalculateMiddlePoint(a, b, weightA, weightB);
            //middlePoint = new Vector2(0.5f*(b.x - a.x), 0.5f * (b.y - a.y));
        }

        public static bool PointEqualsPoint(Vector2 p1, Vector2 p2)
        {
            float epsilon = 0.001f;

            if (Mathf.Abs(p1.x - p2.x) < epsilon && Mathf.Abs(p1.y - p2.y) < epsilon)
            {
                return true;
            }
            return false;
        }

        public static bool ListContainsVector(List<Vector2> list, Vector2 subject, out int index)
        {
            float epsilon = 0.001f;

            for (int i = 0; i < list.Count; i++)
            {
                //if (list[i].x == subject.x && list[i].y == subject.y)
                if (Mathf.Abs(list[i].x - subject.x) < epsilon && Mathf.Abs(list[i].y - subject.y) < epsilon)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public static bool ListContainsVector(List<Vector2> list, Vector2 subject)
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

        public static bool IsBetween(float v, float bound1, float bound2) 
        {
            //WHY ... SRSLY WHY
            float epsilon = 0.0001f;
            return (v >= Mathf.Min(bound1, bound2)-epsilon && v <= Mathf.Max(bound1, bound2) + epsilon);
        }

        public static bool IsBetween(int v, int bound1, int bound2) 
        {
            return (v >= Mathf.Min(bound1, bound2) && v <= Mathf.Max(bound1, bound2));
        }

        public static bool PointOnLineSegment(Vector2 pt1, Vector2 pt2, Vector2 pt, double epsilon = 0.0001)
        {
            if (pt.x - Mathf.Max(pt1.x, pt2.x) > epsilon ||
                Mathf.Min(pt1.x, pt2.x) - pt.x > epsilon ||
                pt.y - Mathf.Max(pt1.y, pt2.y) > epsilon ||
                Mathf.Min(pt1.y, pt2.y) - pt.y > epsilon)
                return false;

            if (Mathf.Abs(pt2.x - pt1.x) < epsilon)
                return Mathf.Abs(pt1.x - pt.x) < epsilon || Mathf.Abs(pt2.x - pt.x) < epsilon;
            if (Mathf.Abs(pt2.y - pt1.y) < epsilon)
                return Mathf.Abs(pt1.y - pt.y) < epsilon || Mathf.Abs(pt2.y - pt.y) < epsilon;

            float x = pt1.x + (pt.y - pt1.y) * (pt2.x - pt1.x) / (pt2.y - pt1.y);
            float y = pt1.y + (pt.x - pt1.x) * (pt2.y - pt1.y) / (pt2.x - pt1.x);

            return Mathf.Abs(pt.x - x) < epsilon || Mathf.Abs(pt.y - y) < epsilon;
        }

        public static bool AngleEqualsAngle(float angle1, float angle2, double epsilon) 
        {
            return (angle1 + epsilon > angle2) && (angle1 - epsilon < angle2);
        }

        /*
        public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            //To avoid floating point precision issues we can add a small value
            float epsilon = 0.00001f;

            bool isIntersecting = false;

            float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            //Make sure the denominator is > 0, if not the lines are parallel
            if (denominator != 0f)
            {
                float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
                float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

                //Are the line segments intersecting if the end points are the same
                if (shouldIncludeEndPoints)
                {
                    //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                    if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    //Is intersecting if u_a and u_b are between 0 and 1
                    if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
            
                if (isIntersecting)
                {
                    intersection = new Vector2((l1_p1.x + u_a*(l1_p2.x- l1_p1.x)), (l1_p1.y + u_a * (l1_p2.y - l1_p1.y)));
                }
            }

            return isIntersecting;
        }
        */
        public static bool LineLineSegmentIntersection(out Vector2 intersection, Vector2 linePoint1, Vector2 lineDirection1, Vector2 lineSegmentPoint1, Vector2 lineSegmentPoint2) 
        {
            //bool result = LineLineIntersection(out Vector2 lineLineIntersection, linePoint1, lineDirection1, lineSegmentPoint1, lineSegmentPoint2 - lineSegmentPoint1);
            bool result = GetIntersection(out Vector2 lineLineIntersection, linePoint1, lineDirection1, lineSegmentPoint1, lineSegmentPoint2 - lineSegmentPoint1);

            /*
            if (lineSegmentPoint1.x == -15)
            {
                Debug.LogWarning("Ergebnis vorher: " + result);
            }
            */

            if (!IsBetween(lineLineIntersection.x, lineSegmentPoint1.x, lineSegmentPoint2.x) || !IsBetween(lineLineIntersection.y, lineSegmentPoint1.y, lineSegmentPoint2.y))
            {
                result = false;
            }

            intersection = lineLineIntersection;

            /*
            if (lineSegmentPoint1.x == -15)
            {
                Debug.LogWarning("Ergebnis: " + result + " --> Teil X: " + IsBetween(lineLineIntersection.x, lineSegmentPoint1.x, lineSegmentPoint2.x) + ", Teil Y: " + IsBetween(lineLineIntersection.y, lineSegmentPoint1.y, lineSegmentPoint2.y));
                Debug.LogWarning("Test: " + IsBetween(-19, -19, -19) + " / " + IsBetween(19, 19, 19) + " [" + lineLineIntersection.y.ToString("F10") + ", " + lineSegmentPoint1.y.ToString("F10") + ", " + lineSegmentPoint2.y.ToString("F10") + " --> " + IsBetween(lineLineIntersection.y, lineSegmentPoint1.y, lineSegmentPoint2.y) +  "]");
                Debug.LogWarning("TEST2 : " + (lineLineIntersection.y >= Mathf.Min(lineSegmentPoint1.y, lineSegmentPoint2.y)) + ", " + (lineLineIntersection.y <= Mathf.Max(lineSegmentPoint1.y, lineSegmentPoint2.y)) + " gesammt: " + ((lineLineIntersection.y >= Mathf.Min(lineSegmentPoint1.y, lineSegmentPoint2.y)) && (lineLineIntersection.y <= Mathf.Min(lineSegmentPoint1.y, lineSegmentPoint2.y))));
                Debug.LogWarning("TEST3: " + Mathf.Min(lineSegmentPoint1.y, lineSegmentPoint2.y).ToString("F10"));
                Debug.LogWarning("TEST3: " + (lineLineIntersection.y >= Mathf.Min(lineSegmentPoint1.y, 30)) + ", 2.: " + (lineLineIntersection.y >= Mathf.Min(30, lineSegmentPoint2.y)) + ", ABER-->Vergleich: " + (lineLineIntersection.y >= 19) + " / " + lineLineIntersection.y.ToString("F40"));
            }
            */

            return result;
        }

        public static bool LineLineIntersection(out Vector2 intersection, Vector2 linePoint1, Vector2 lineDirection1, Vector2 linePoint2, Vector2 lineDirection2) 
        {
            bool result = LineLineIntersection(out Vector3 intersection3, new Vector3(linePoint1.x, 0, linePoint1.y), new Vector3(lineDirection1.normalized.x, 0, lineDirection1.normalized.y), new Vector3(linePoint2.x, 0, linePoint2.y), new Vector3(lineDirection2.normalized.x, 0, lineDirection2.normalized.y));
            //Debug.Log("( " + linePoint1 + " - " + lineDirection1.normalized + " - " + linePoint2 + " - " + lineDirection2.normalized + ")");
            intersection = new Vector2(intersection3.x, intersection3.z);
            return result;
        }

        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineDirection1, Vector3 linePoint2, Vector3 lineDirection2)
        {
            /*
            float epsilon = 0.0001f;

            Vector2 p1 = new Vector2(linePoint1.x, linePoint1.z);
            Vector2 p2 = new Vector2(linePoint1.x + lineDirection1.x, linePoint1.z + lineDirection1.z);
            Vector2 p3 = new Vector2(linePoint2.x, linePoint2.z);
            Vector2 p4 = new Vector2(linePoint2.x + lineDirection2.x, linePoint2.z + lineDirection2.z);

            float denominator = ((p1.x - p2.x) * (p3.y - p4.y)) - ((p1.y - p2.y) * (p3.x - p4.x));

            Vector2 inter;
            if (denominator < epsilon)
            {
                Debug.LogWarning("ABBRUCH");
                inter = Vector2.zero;
                //return false;
            }

            float p_x = ((((p1.x * p2.y) - (p1.y * p2.x)) * (p3.x - p4.x)) - ((p1.x - p2.x) * ((p3.x * p4.y) - (p3.y * p4.x)))) / denominator;
            float p_y = ((((p1.x * p2.y) - (p1.y * p2.x)) * (p3.y - p4.y)) - ((p1.y - p2.y) * ((p3.x * p4.y) - (p3.y * p4.x)))) / denominator;

            //float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
            //float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

            //float IntersectionX = p1.x + u_a * (p2.x - p1.x);
            //float IntersectionY = p1.y + u_a * (p2.y - p1.y);
            inter = new Vector2(p_x, p_y);

            //return true;
            */
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            /*
            intersection = linePoint1 + (lineDirection1 * (Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude));
            Debug.LogError(intersection + " < > " + inter);
            if (intersection.x == inter.x) 
            {
            
            }
            */

            //is coplanar, and not parallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineDirection1 * s);
                return true;
            }
            else
            {
                //Debug.Log(planarFactor + " <> " + crossVec1and2.sqrMagnitude);
                //Debug.Log("( " + linePoint1 + " - " + lineDirection1 + " - " + linePoint2 + " - " + lineDirection2 + ")");
                intersection = Vector3.zero;
                return false;
            }
        }

        /*
        public static bool lineLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 p)
        {
            const float EPSILON = 1E-6f;
            p = new Vector2(float.NaN, float.NaN);

            var cc = (a1.y - a2.y) * (b2.x - b1.x) - (a2.x - a1.x) * (b1.y - b2.y);
            if (System.Math.Abs(cc) < EPSILON) return false; // lines are parallel or congruent

            p = (1f / cc) * new Vector2(
              (a2.x - a1.x) * (b1.x * b2.y - b1.y * b2.x) - (a1.x * a2.y - a1.y * a2.x) * (b2.x - b1.x),
              (a1.x * a2.y - a1.y * a2.x) * (b1.y - b2.y) - (a1.y - a2.y) * (b1.x * b2.y - b1.y * b2.x)
            );

            return true;
        }
        */

        public static bool GetIntersection(out Vector2 intersection, Vector2 A, Vector2 a, Vector2 B, Vector2 b)
        {
            float epsilon = 0.0001f;

            Vector2 p1 = A;
            Vector2 p2 = A + a;

            Vector2 p3 = B;
            Vector2 p4 = B + b;

            //float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
            float denominator = ((p1.x - p2.x) * (p3.y - p4.y)) - ((p1.y - p2.y) * (p3.x - p4.x));
            
            if (Mathf.Abs(denominator) < epsilon)
            {
                //Debug.LogWarning("Intersection is coplanar or parallel");
                intersection = Vector2.zero;
                return false;
            }
            
            float p_x = ((((p1.x*p2.y)-(p1.y*p2.x)) * (p3.x-p4.x)) - ((p1.x-p2.x) * ((p3.x*p4.y)-(p3.y*p4.x))))/ denominator;
            float p_y = ((((p1.x*p2.y)-(p1.y*p2.x)) * (p3.y-p4.y)) - ((p1.y-p2.y) * ((p3.x*p4.y)-(p3.y*p4.x)))) / denominator;

            //float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
            //float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

            //float IntersectionX = p1.x + u_a * (p2.x - p1.x);
            //float IntersectionY = p1.y + u_a * (p2.y - p1.y);
            intersection = new Vector2(p_x, p_y);

            return true;
        }


        public static Vector3 ClosestPointOnLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point)
        {
            // Shift the problem to the origin to simplify the math.    
            var wander = point - segmentStart;
            var span = segmentEnd - segmentStart;

            // Compute how far along the line is the closest approach to our point.
            float t = Vector3.Dot(wander, span) / span.sqrMagnitude;

            // Restrict this point to within the line segment from start to end.
            t = Mathf.Clamp01(t);

            // Return this point.
            return segmentStart + t * span;
        }

        public static Vector2 ClosestPointOnLineSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
        {
            // Shift the problem to the origin to simplify the math.    
            var wander = point - segmentStart;
            var span = segmentEnd - segmentStart;

            // Compute how far along the line is the closest approach to our point.
            float t = Vector2.Dot(wander, span) / span.sqrMagnitude;

            // Restrict this point to within the line segment from start to end.
            t = Mathf.Clamp01(t);

            // Return this point.
            return segmentStart + t * span;
        }

        public static float DistancePointToLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point) 
        {
            Vector3 closestPoint = ClosestPointOnLineSegment(segmentStart, segmentEnd, point);

            return Vector3.Distance(point, closestPoint);
        }
    }
}