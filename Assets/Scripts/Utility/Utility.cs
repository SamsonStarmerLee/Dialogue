// Last Updated: 23/06/2020

namespace Framework.Maths
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Collection of math-y functions that don't fit anywhere else.
    /// </summary>
    public static class Utility
    {
        public const float Tolerance = 0.0001f;
        public const float Pi = 3.141592f;
        public const float Tau = Pi * 2;

        /// <summary>
        /// Map a number in one range to another range.
        /// </summary>
        public static float Map(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            if (Math.Abs(oldMin - oldMax) > Tolerance && Math.Abs(newMin - newMax) > Tolerance)
            {
                return (value - oldMin) * (newMax - newMin) / (oldMax - oldMin) + newMin;
            }

            return (newMax + newMin) / 2f;
        }

        /// <summary>
        /// Performs an inverse lerp with no bounds checking.
        /// </summary>
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }

        /// <summary>
        /// Access an array of floats granularly.
        /// </summary>
        public static float Sample(this float[] arr, float t)
        {
            var count = arr.Length;

            switch (count)
            {
                case 0:
                    Debug.LogError("Unable to sample array - is has no elements");
                    return 0;
                case 1:
                    return arr[0];
            }

            var iFloat = t * (count - 1);
            var idLower = Mathf.FloorToInt(iFloat);
            var idUpper = Mathf.FloorToInt(iFloat + 1);

            if (idUpper >= count)
            {
                return arr[count - 1];
            }

            if (idLower < 0)
            {
                return arr[0];
            }

            return Mathf.Lerp(arr[idLower], arr[idUpper], iFloat - idLower);
        }

        /// <summary>
        /// Checks if a given point is above the defined plane.
        /// </summary>
        /// <param name="point">The point to test against the plane.</param>
        /// <param name="planeNormal">Normal of the plane.</param>
        /// <param name="planePoint">Position of the plane.</param>
        /// <returns></returns>
        public static bool PointAbovePlane(Vector3 point, Vector3 planeNormal, Vector3 planePoint)
        {
            var direction = point - planePoint;
            return Vector3.Angle(direction, planeNormal) < 90f;
        }

        /// <summary>
        /// Clamps an angle to within the 360 degree circle.
        /// </summary>
        /// <param name="angle">The angle to clamp.</param>
        public static float ClampAngle(float angle)
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
            return angle;
        }

        /// <summary>
        /// Clamps an angle to within a full rotation.
        /// </summary>
        /// <param name="angle">The angle to clamp.</param>
        public static float ClampAngleRadians(float angle)
        {
            if (angle < -Tau)
            {
                angle += Tau;
            }
            if (angle > Tau)
            {
                angle -= Tau;
            }
            return angle;
        }

        /// <summary>
        /// Return a quaternion which rotates an object so its X axis points towards 'forward'.
        /// </summary>
        public static Quaternion GetRotationXForward(Vector2 forward)
        {
            var angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }

        /// <summary>
        /// Snaps a vector on one axis to increments of the given angle.
        /// Can be used to simulate controller 'notches'.
        /// </summary>
        /// <param name="this">The vector to snap.</param>
        /// <param name="snapAngle">Angle to snap at. In degrees.</param>
        /// <param name="forward">Vector paralell to the axis the vector is on (on the same plane).</param>
        public static Vector3 SnapToAngles(this Vector3 @this, float snapAngle, Vector3 forward)
        {
            if (snapAngle <= 0 || @this == Vector3.zero || forward == Vector3.zero)
            {
                return @this;
            }

            var angle = Vector3.Angle(@this, forward);

            // Cannot do cross product with angles of 0 && 180.
            if (angle < snapAngle / 2f)
                return forward * @this.magnitude;
            if (angle > 180.0f - snapAngle / 2f)
                return -forward * @this.magnitude;

            var t = Mathf.Round(angle / snapAngle);
            var deltaAngle = (t * snapAngle) - angle;

            var axis = Vector3.Cross(forward, @this);
            var q = Quaternion.AngleAxis(deltaAngle, axis);

            return q * @this;
        }

        /// <summary>
        /// Returns the given value taken from 1.
        /// </summary>
        public static float OneMinus(this float @this)
        {
            return 1 - @this;
        }

        /// <summary>
        /// Returns true if the given index is a valid one within the array.
        /// </summary>
        public static bool IsValidIndex(this Array @this, int index)
        {
            return index >= 0 && index < @this.Length;
        }

        /// <summary>
        /// Returns true if the given index is a valid one within the list.
        /// </summary>
        public static bool IsValidIndex(this IList @this, int index)
        {
            return index >= 0 && index < @this.Count;
        }

        /// <summary>
        /// Returns true if the given index is a valid one within the array.
        /// </summary>
        public static bool IsValidIndex<T>(this T[] @this, int index)
        {
            return index >= 0 && index < @this.Length;
        }

        /// <summary>
        /// Returns true if the given index is a valid one within the array.
        /// </summary>
        public static bool IsValidIndex<T>(this T[,] @this, int x, int y)
        {
            if (x < 0 || x >= @this.GetLength(0)) return false;
            if (y < 0 || y >= @this.GetLength(1)) return false;
            return true;
        }

        /// <summary>
        /// Returns a list with references to all valid cardinal neighbours in a 2D array.
        /// </summary>
        public static List<T> GetCardinalNeighboursOf<T>(this T[,] @this, int x, int y)
        {
            var neighbours = new List<T>();

            if (x + 1 < @this.GetLength(0))
            {
                neighbours.Add(@this[x + 1, y]);
            }

            if (x - 1 >= 0)
            {
                neighbours.Add(@this[x - 1, y]);
            }

            if (y + 1 < @this.GetLength(1))
            {
                neighbours.Add(@this[x, y + 1]);
            }

            if (y - 1 >= 0)
            {
                neighbours.Add(@this[x, y - 1]);
            }

            return neighbours;
        }

        /// <summary>
        /// Returns a list with references to all valid neighbours in a 2D array.
        /// </summary>
        public static List<T> GetAllNeighboursOf<T>(this T[,] @this, int x, int y)
        {
            var neighbours = new List<T>();

            var up = @this.IsValidIndex(x, y - 1);
            var down = @this.IsValidIndex(x, y + 1);
            var left = @this.IsValidIndex(x - 1, y);
            var right = @this.IsValidIndex(x + 1, y);

            // We test each straight direction, then subtest the next one clockwise.
            if (left)
            {
                neighbours.Add(@this[x - 1, y]);

                if (up && @this.IsValidIndex(x - 1, y - 1))
                {
                    neighbours.Add(@this[x - 1, y - 1]);
                }
            }

            if (up)
            {
                neighbours.Add(@this[x, y - 1]);

                if (right && @this.IsValidIndex(x + 1, y - 1))
                {
                    neighbours.Add(@this[x + 1, y - 1]);
                }
            }

            if (right)
            {
                neighbours.Add(@this[x + 1, y]);

                if (down && @this.IsValidIndex(x + 1, y + 1))
                {
                    neighbours.Add(@this[x + 1, y + 1]);
                }
            }

            if (down)
            {
                neighbours.Add(@this[x, y + 1]);

                if (left && @this.IsValidIndex(x - 1, y + 1))
                {
                    neighbours.Add(@this[x - 1, y + 1]);
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Returns a list with references to all valid cardinal neighbours in a 2D array.
        /// </summary>
        public static List<(int Row, int Col)> GetValidCardinalIndices<T>(this T[,] @this, int x, int y)
        {
            var neighbours = new List<(int Row, int Col)>();

            if (x + 1 < @this.GetLength(0))
            {
                neighbours.Add((x + 1, y));
            }

            if (x - 1 >= 0)
            {
                neighbours.Add((x - 1, y));
            }

            if (y + 1 < @this.GetLength(1))
            {
                neighbours.Add((x, y + 1));
            }

            if (y - 1 >= 0)
            {
                neighbours.Add((x, y - 1));
            }

            return neighbours;
        }

        /// <summary>
        /// Returns a list with references to all valid neighbours in a 2D array.
        /// </summary>
        public static List<(int Row, int Col)> GetValidAdjacentIndices<T>(this T[,] @this, int x, int y)
        {
            var neighbours = new List<(int Row, int Col)>();

            var up = @this.IsValidIndex(x, y - 1);
            var down = @this.IsValidIndex(x, y + 1);
            var left = @this.IsValidIndex(x - 1, y);
            var right = @this.IsValidIndex(x + 1, y);

            // We test each straight direction, then subtest the next one clockwise.
            if (left)
            {
                neighbours.Add((x - 1, y));

                if (up && @this.IsValidIndex(x - 1, y - 1))
                {
                    neighbours.Add((x - 1, y - 1));
                }
            }

            if (up)
            {
                neighbours.Add((x, y - 1));

                if (right && @this.IsValidIndex(x + 1, y - 1))
                {
                    neighbours.Add((x + 1, y - 1));
                }
            }

            if (right)
            {
                neighbours.Add((x + 1, y));

                if (down && @this.IsValidIndex(x + 1, y))
                {
                    neighbours.Add((x + 1, y + 1));
                }
            }

            if (down)
            {
                neighbours.Add((x, y + 1));

                if (left && @this.IsValidIndex(x, y + 1))
                {
                    neighbours.Add((x - 1, y + 1));
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Zip Linq function is missing from unity's version of mono :(
        /// </summary>
        public static IEnumerable<TResult> Zip<TA, TB, TResult>(
            this IEnumerable<TA> seqA, IEnumerable<TB> seqB, Func<TA, TB, TResult> func)
        {
            if (seqA == null)
            {
                throw new ArgumentNullException("seqA");
            }
            if (seqB == null)
            {
                throw new ArgumentNullException("seqB");
            }

            using (var iteratorA = seqA.GetEnumerator())
            using (var iteratorB = seqB.GetEnumerator())
            {
                while (iteratorA.MoveNext() && iteratorB.MoveNext())
                    yield return func(iteratorA.Current, iteratorB.Current);
            }
        }

        /// <summary>
        /// Randomizes an array in-place.
        /// </summary>
        public static void Shuffle<T>(this T[] @this)
        {
            var n = @this.Length;

            while (n > 1)
            {
                n--;
                var k = Random.Range(0, n + 1);
                var value = @this[k];
                @this[k] = @this[n];
                @this[n] = value;
            }
        }

        /// <summary>
        /// Randomizes a list in-place.
        /// </summary>
        public static void Shuffle<T>(this List<T> @this)
        {
            var n = @this.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = @this[k];
                @this[k] = @this[n];
                @this[n] = value;
            }
        }

        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);

        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);

        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

        public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);

        public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);

        public static Vector3 WithZ(this Vector2 v, float z) => new Vector3(v.x, v.y, z);

        public static Vector2 Rotate(this Vector2 @this, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var _x = @this.x * Mathf.Cos(radians) - @this.y * Mathf.Sin(radians);
            var _y = @this.x * Mathf.Sin(radians) + @this.y * Mathf.Cos(radians);
            return new Vector2(_x, _y);
        }

        /// <summary>
        /// Returns a perpendicular 'right' vector to the one given.
        /// Can serve as a 2D equivalent to the 3D 'Cross' function.
        /// </summary>
        public static Vector2 Righthand(this Vector2 @this) =>
            new Vector2(@this.y, -@this.x);

        public static bool IsNaN(this Vector3 vect) =>
            Single.IsNaN(vect.x) ||
            Single.IsNaN(vect.y) ||
            Single.IsNaN(vect.z);

        public static bool IsNaN(this Vector2 vect) =>
            Single.IsNaN(vect.x) ||
            Single.IsNaN(vect.y);

        /// <summary>
        /// Increase or decrease the length of vector by size.
        /// </summary>
        public static Vector3 AddVectorLength(this Vector3 @this, float size)
        {
            var magnitude = @this.magnitude + size;
            return Vector3.Scale(@this.normalized, new Vector3(magnitude, magnitude, magnitude));
        }

        /// <summary>
        /// Create a vector of direction "vector" with length "size".
        /// </summary>
        public static Vector3 SetVectorLength(this Vector3 @this, float size)
        {
            return @this.normalized * size;
        }

        /// <summary>
        /// Increase or decrease the length of vector by size.
        /// </summary>
        public static Vector2 AddVectorLength(this Vector2 @this, float size)
        {
            var magnitude = @this.magnitude + size;
            return Vector2.Scale(@this.normalized, new Vector2(magnitude, magnitude));
        }

        /// <summary>
        /// Create a vector of direction "vector" with length "size".
        /// </summary>
        public static Vector2 SetVectorLength(this Vector2 @this, float size)
        {
            return @this.normalized * size;
        }

        /// <summary>
        /// Caclulate the rotational difference from A to B.
        /// </summary>
        /// <param name="B"></param>
        /// <param name="A"></param>
        public static Quaternion SubtractRotation(Quaternion B, Quaternion A)
        {
            var C = Quaternion.Inverse(A) * B;
            return C;
        }

        /// <summary>
        /// Find the line of intersection between two planes.	
        /// The planes are defined by a normal and a point on that plane.
        /// The outputs are a point on the line and a vector which indicates it's direction. 
        /// If the planes are not parallel, the function outputs true, otherwise false.
        /// </summary>
        public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal,
            Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position)
        {
            linePoint = Vector3.zero;

            // We can get the direction of the line of intersection of the two planes by calculating the
            // cross product of the normals of the two planes. Note that this is just a direction and the line
            // is not fixed in space yet. We need a point for that to go with the line vector.
            lineVec = Vector3.Cross(plane1Normal, plane2Normal);

            // Next is to calculate a point on the line to fix it's position in space. This is done by finding a vector from
            // the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
            // errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
            // the cross product of the normal of plane2 and the lineDirection.
            var ldir = Vector3.Cross(plane2Normal, lineVec);

            var denominator = Vector3.Dot(plane1Normal, ldir);

            //Prevent divide by zero and rounding errors by requiring about 5 degrees angle between the planes.
            if (Mathf.Abs(denominator) > 0.006f)
            {
                var plane1ToPlane2 = plane1Position - plane2Position;
                var t = Vector3.Dot(plane1Normal, plane1ToPlane2) / denominator;
                linePoint = plane2Position + t * ldir;

                return true;
            }

            // output not valid
            return false;
        }

        /// <summary>
        /// Get the intersection between a line and a plane.
        /// If the line and plane are not parallel, the function outputs true, otherwise false.
        /// </summary>
        public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec,
            Vector3 planeNormal, Vector3 planePoint)
        {
            float length;
            float dotNumerator;
            float dotDenominator;
            Vector3 vector;
            intersection = Vector3.zero;

            //calculate the distance between the linePoint and the line-plane intersection point
            dotNumerator = Vector3.Dot(planePoint - linePoint, planeNormal);
            dotDenominator = Vector3.Dot(lineVec, planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;

                //create a vector from the linePoint to the intersection point
                vector = SetVectorLength(lineVec, length);

                //get the coordinates of the line-plane intersection point
                intersection = linePoint + vector;

                return true;
            }

            //output not valid
            return false;
        }

        /// <summary>
        /// Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
        /// Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the
        /// same plane, use ClosestPointsOnTwoLines() instead.
        /// </summary>
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1,
            Vector3 linePoint2, Vector3 lineVec2)
        {
            intersection = Vector3.zero;

            var lineVec3 = linePoint2 - linePoint1;
            var crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            var crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            var planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //Lines are not coplanar. Take into account rounding errors.
            if (planarFactor >= 0.00001f || planarFactor <= -0.00001f)
                return false;

            //Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
            var s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

            if (s >= 0.0f && s <= 1.0f)
            {
                intersection = linePoint1 + lineVec1 * s;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Two non-parallel lines which may or may not touch each other have a point on each line which are closest
        /// to each other.This function finds those two points.If the lines are not parallel, the function
        /// outputs true, otherwise false.
        /// </summary>
        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2,
            Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            var a = Vector3.Dot(lineVec1, lineVec1);
            var b = Vector3.Dot(lineVec1, lineVec2);
            var e = Vector3.Dot(lineVec2, lineVec2);

            var d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {
                var r = linePoint1 - linePoint2;
                var c = Vector3.Dot(lineVec1, r);
                var f = Vector3.Dot(lineVec2, r);

                var s = (b * f - c * e) / d;
                var t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }

            return false;
        }

        /// <summary>
        /// This function returns a point which is a projection from a point to a line.
        /// The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
        /// </summary>
        public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
        {
            //get vector from point on line to point in space
            var linePointToPoint = point - linePoint;

            var t = Vector3.Dot(linePointToPoint, lineVec);

            return linePoint + lineVec * t;
        }

        /// <summary>
        /// This function returns a point which is a projection from a point to a line segment.
        /// If the projected point lies outside of the line segment, the projected point will
        /// be clamped to the appropriate line edge.
        /// If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
        /// </summary>
        public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            var vector = linePoint2 - linePoint1;

            var projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

            var side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

            //The projected point is on the line segment
            if (side == 0)
                return projectedPoint;

            if (side == 1)
                return linePoint1;

            if (side == 2)
                return linePoint2;

            //output is invalid
            return Vector3.zero;
        }

        /// <summary>
        /// This function returns a point which is a projection from a point to a plane.
        /// </summary>
        public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
        {
            //First calculate the distance from the point to the plane:
            var distance = SignedDistancePlanePoint(planeNormal, planePoint, point);

            //Reverse the sign of the distance
            distance *= -1;

            //Get a translation vector
            var translationVector = SetVectorLength(planeNormal, distance);

            //Translate the point to form a projection
            return point + translationVector;
        }

        /// <summary>
        /// Projects a vector onto a plane. The output is not normalized.
        /// </summary>
        public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
        {
            return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
        }

        /// <summary>
        /// Projects a vector onto a plane, scaling the resulting vector to have the same magnitude as the input.
        /// </summary>
        public static Vector3 ProjectVectorOnPlaneRescaled(Vector3 planeNormal, Vector3 vector)
        {
            var projected = vector - planeNormal * Vector3.Dot(vector, planeNormal);
            return projected.normalized * vector.magnitude;
        }

        /// <summary>
        /// Projects a vector onto a plane. The output is not normalized.
        /// </summary>
        public static Vector2 ProjectVectorOnPlane(Vector2 planeNormal, Vector2 vector)
        {
            return vector - planeNormal * Vector2.Dot(vector, planeNormal);
        }

        /// <summary>
        /// Projects a vector onto a plane, scaling the resulting vector to have the same magnitude as the input.
        /// </summary>
        public static Vector2 ProjectVectorOnPlaneRescaled(Vector2 planeNormal, Vector2 vector)
        {
            var projected = vector - planeNormal * Vector2.Dot(vector, planeNormal);
            return projected.normalized * vector.magnitude;
        }

        /// <summary>
        /// Get the shortest distance between a point and a plane. The output is signed so it holds information
        /// as to which side of the plane normal the point is.
        /// </summary>
        public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
        {
            return Vector3.Dot(planeNormal, point - planePoint);
        }

        /// <summary>
        /// Calculate the angle between a vector and a plane. The plane is made by a normal vector.
        /// Output is in radians.
        /// </summary>
        public static float AngleVectorPlane(Vector3 vector, Vector3 normal)
        {
            //calculate the the dot product between the two input vectors. This gives the cosine between the two vectors
            var dot = Vector3.Dot(vector, normal);

            //this is in radians
            var angle = (float)System.Math.Acos(dot);

            return 1.570796326794897f - angle; //90 degrees - angle
        }

        /// <summary>
        /// Calculate the dot product as an angle
        /// </summary>
        public static float DotProductAngle(Vector3 vec1, Vector3 vec2)
        {
            // Get the dot product
            double dot = Vector3.Dot(vec1, vec2);

            // Clamp to prevent NaN error. Shouldn't need this in the first place, but there could be a rounding error issue.
            if (dot < -1.0f)
                dot = -1.0f;
            if (dot > 1.0f)
                dot = 1.0f;

            // Calculate the angle. The output is in radians
            // This step can be skipped for optimization...
            var angle = System.Math.Acos(dot);

            return (float)angle;
        }

        /// <summary>
        /// Convert a plane defined by 3 points to a plane defined by a vector and a point.
        /// The plane point is the middle of the triangle defined by the 3 points.
        /// </summary>
        public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA,
            Vector3 pointB, Vector3 pointC)
        {
            // Make two vectors from the 3 input points, originating from point A.
            var AB = pointB - pointA;
            var AC = pointC - pointA;

            // Calculate the normal.
            planeNormal = Vector3.Normalize(Vector3.Cross(AB, AC));

            // Get the points in the middle AB and AC.
            var middleAB = pointA + AB / 2.0f;
            var middleAC = pointA + AC / 2.0f;

            // Get vectors from the middle of AB and AC to the point which is not on that line.
            var middleABtoC = pointC - middleAB;
            var middleACtoB = pointB - middleAC;

            // Calculate the intersection between the two lines. This will be the center
            // of the triangle defined by the 3 points.
            // We could use LineLineIntersection instead of ClosestPointsOnTwoLines but due to rounding errors
            // this sometimes doesn't work.
            ClosestPointsOnTwoLines(out planePoint, out Vector3 temp, middleAB, middleABtoC, middleAC, middleACtoB);
        }

        /// <summary>
        /// Returns the forward vector of a quaternion.
        /// </summary>
        public static Vector3 GetForwardVector(Quaternion q)
        {
            return q * Vector3.forward;
        }

        /// <summary>
        /// Returns the up vector of a quaternion.
        /// </summary>
        public static Vector3 GetUpVector(Quaternion q)
        {
            return q * Vector3.up;
        }

        /// <summary>
        /// Returns the right vector of a quaternion.
        /// </summary>
        public static Vector3 GetRightVector(Quaternion q)
        {
            return q * Vector3.right;
        }

        /// <summary>
        /// Gets a quaternion from a matrix
        /// </summary>
        public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }

        /// <summary>
        /// Gets a position from a matrix
        /// </summary>
        public static Vector3 PositionFromMatrix(Matrix4x4 m)
        {
            var vector4Position = m.GetColumn(3);
            return new Vector3(vector4Position.x, vector4Position.y, vector4Position.z);
        }

        /// <summary>
        /// This is an alternative for Quaternion.LookRotation. Instead of aligning the forward and up vector of the game
        /// object with the input vectors, a custom direction can be used instead of the fixed forward and up vectors.
        /// alignWithVector and alignWithNormal are in world space.
        /// customForward and customUp are in object space.
        /// Usage: use alignWithVector and alignWithNormal as if you are using the default LookRotation function.
        /// Set customForward and customUp to the vectors you wish to use instead of the default forward and up vectors.
        /// </summary>
        public static void LookRotationExtended(ref GameObject gameObjectInOut, Vector3 alignWithVector,
            Vector3 alignWithNormal, Vector3 customForward, Vector3 customUp)
        {
            // Set the rotation of the destination
            var rotationA = Quaternion.LookRotation(alignWithVector, alignWithNormal);

            // Set the rotation of the custom normal and up vectors.
            // When using the default LookRotation function, this would be hard coded to the forward and up vector.
            var rotationB = Quaternion.LookRotation(customForward, customUp);

            // Calculate the rotation
            gameObjectInOut.transform.rotation = rotationA * Quaternion.Inverse(rotationB);
        }

        /// <summary>
        /// With this function you can align a triangle of an object with any transform.
        /// Usage: gameObjectInOut is the game object you want to transform.
        /// alignWithVector, alignWithNormal, and alignWithPosition is the transform with which the triangle of the object should be aligned with.
        /// triangleForward, triangleNormal, and trianglePosition is the transform of the triangle from the object.
        /// alignWithVector, alignWithNormal, and alignWithPosition are in world space.
        /// triangleForward, triangleNormal, and trianglePosition are in object space.
        /// trianglePosition is the mesh position of the triangle. The effect of the scale of the object is handled automatically.
        /// trianglePosition can be set at any position, it does not have to be at a vertex or in the middle of the triangle.
        /// </summary>
        public static void PreciseAlign(ref GameObject gameObjectInOut, Vector3 alignWithVector, Vector3 alignWithNormal,
            Vector3 alignWithPosition, Vector3 triangleForward, Vector3 triangleNormal, Vector3 trianglePosition)
        {
            //Set the rotation.
            LookRotationExtended(ref gameObjectInOut, alignWithVector, alignWithNormal, triangleForward, triangleNormal);

            //Get the world space position of trianglePosition
            var trianglePositionWorld = gameObjectInOut.transform.TransformPoint(trianglePosition);

            //Get a vector from trianglePosition to alignWithPosition
            var translateVector = alignWithPosition - trianglePositionWorld;

            //Now transform the object so the triangle lines up correctly.
            gameObjectInOut.transform.Translate(translateVector, Space.World);
        }

        /// <summary>
        /// Convert a position, direction, and normal vector to a transform
        /// </summary>
        private static void VectorsToTransform(ref GameObject gameObjectInOut, Vector3 positionVector,
            Vector3 directionVector, Vector3 normalVector)
        {
            gameObjectInOut.transform.position = positionVector;
            gameObjectInOut.transform.rotation = Quaternion.LookRotation(directionVector, normalVector);
        }

        /// <summary>
        /// This function finds out on which side of a line segment the point is located.
        /// The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
        /// the line segment, project it on the line using ProjectPointOnLine() first.
        /// Returns 0 if point is on the line segment.
        /// Returns 1 if point is outside of the line segment and located on the side of linePoint1.
        /// Returns 2 if point is outside of the line segment and located on the side of linePoint2.
        /// </summary>
        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            var lineVec = linePoint2 - linePoint1;
            var pointVec = point - linePoint1;

            var dot = Vector3.Dot(pointVec, lineVec);

            //point is on side of linePoint2, compared to linePoint1
            if (!(dot > 0)) return 1;
            if (pointVec.magnitude <= lineVec.magnitude)
                return 0;

            //point is not on the line segment and it is on the side of linePoint2
            else
                return 2;

            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
        }

        /// <summary>
        /// Returns true if a line segment (made up of linePoint1 and linePoint2) is fully or partially in a rectangle
        /// made up of RectA to RectD. The line segment is assumed to be on the same plane as the rectangle. If the line is
        /// not on the plane, use ProjectPointOnPlane() on linePoint1 and linePoint2 first.
        /// </summary>
        public static bool IsLineInRectangle(Vector3 linePoint1, Vector3 linePoint2, Vector3 rectA, Vector3 rectB,
            Vector3 rectC, Vector3 rectD)
        {
            var pointAInside = false;
            var pointBInside = false;

            pointAInside = IsPointInRectangle(linePoint1, rectA, rectC, rectB, rectD);

            if (!pointAInside)
                pointBInside = IsPointInRectangle(linePoint2, rectA, rectC, rectB, rectD);

            //none of the points are inside, so check if a line is crossing
            if (!pointAInside && !pointBInside)
            {
                var lineACrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectA, rectB);
                var lineBCrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectB, rectC);
                var lineCCrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectC, rectD);
                var lineDCrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectD, rectA);

                if (lineACrossing || lineBCrossing || lineCCrossing || lineDCrossing)
                    return true;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if "point" is in a rectangle mad up of RectA to RectD. The line point is assumed to be on the same
        /// plane as the rectangle. If the point is not on the plane, use ProjectPointOnPlane() first.
        /// </summary>
        public static bool IsPointInRectangle(Vector3 point, Vector3 rectA, Vector3 rectC, Vector3 rectB, Vector3 rectD)
        {
            //get the center of the rectangle
            var vector = rectC - rectA;
            var size = -(vector.magnitude / 2f);
            vector = AddVectorLength(vector, size);
            var middle = rectA + vector;

            var xVector = rectB - rectA;
            var width = xVector.magnitude / 2f;

            var yVector = rectD - rectA;
            var height = yVector.magnitude / 2f;

            var linePoint = ProjectPointOnLine(middle, xVector.normalized, point);
            vector = linePoint - point;
            var yDistance = vector.magnitude;

            linePoint = ProjectPointOnLine(middle, yVector.normalized, point);
            vector = linePoint - point;
            var xDistance = vector.magnitude;

            if (xDistance <= width && yDistance <= height)
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if line segment made up of pointA1 and pointA2 is crossing line segment made up of
        /// pointB1 and pointB2. The two lines are assumed to be in the same plane.
        /// </summary>
        public static bool AreLineSegmentsCrossing(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2)
        {

            var lineVecA = pointA2 - pointA1;
            var lineVecB = pointB2 - pointB1;

            var valid = ClosestPointsOnTwoLines(out Vector3 closestPointA, out Vector3 closestPointB, pointA1, lineVecA.normalized,
                pointB1, lineVecB.normalized);

            //lines are not parallel
            if (valid)
            {
                var sideA = PointOnWhichSideOfLineSegment(pointA1, pointA2, closestPointA);
                var sideB = PointOnWhichSideOfLineSegment(pointB1, pointB2, closestPointB);

                if (sideA == 0 && sideB == 0)
                    return true;

                return false;
            }

            //lines are parallel
            return false;
        }

        /// <summary>
        /// Checks if the duration since start time has elapsed.
        /// </summary>
        public static bool Elapsed(float startTime, float duration)
        {
            return Time.time > startTime + duration;
        }

        /// <summary>
        /// Snaps a Vector2 to only the cardinal direcitons.
        /// </summary>
        public static Vector2 SnapToCardinal(this Vector2 @this, float snapAngle)
        {
            if (snapAngle <= 0 || @this == Vector2.zero)
            {
                return @this;
            }

            var absX = Mathf.Abs(@this.x);
            var absY = Mathf.Abs(@this.y);

            if (absX > absY)
                return new Vector2(@this.x, 0);
            else
                return new Vector2(0, @this.y);
        }

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            UpLeft,
            UpRight,
            DownLeft,
            DownRight,
        }

        static readonly Direction[] cardinalHeadings =
        {
            Direction.Right,
            Direction.Up,
            Direction.Left,
            Direction.Down,
        };

        /// <summary>
        /// Returns an enum indicating the cardinal direction most aligned with the given vector.
        /// </summary>
        public static Direction ToCardinalDirection(this Vector2 @this)
        {
            var angle = Mathf.Atan2(@this.y, @this.x) * Mathf.Rad2Deg;
            var div = 360f / 4f;
            var quadrant = Mathf.RoundToInt(angle / div + 4f) % 4;
            return cardinalHeadings[quadrant];
        }

        static readonly Direction[] cardinalOrdinalHeadings =
        {
            Direction.Right,
            Direction.UpRight,
            Direction.Up,
            Direction.UpLeft,
            Direction.Left,
            Direction.DownLeft,
            Direction.Down,
            Direction.DownRight,
        };

        /// <summary>
        /// Returns an enum indicating the cardinal or ordinal direction most aligned with the given vector.
        /// </summary>
        public static Direction ToCardinalOrdinalDirection(this Vector2 @this)
        {
            var angle = Mathf.Atan2(@this.y, @this.x) * Mathf.Rad2Deg;
            var div = 360f / 8f;
            var quadrant = Mathf.RoundToInt(angle / div + 8f) % 8;
            return cardinalOrdinalHeadings[quadrant];
        }
    }
}