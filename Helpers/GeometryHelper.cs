﻿using System;
using Microsoft.Xna.Framework;

namespace Randio_2 {
    //Contains methods to assist with geometry-related problems - intersections, angle conversions, etc.
    class GeometryHelper
    {
        #region Public methods
        //Returns the depth of an intersection of two rectangles (0 if they don't intersect)
        public static Vector2 GetIntersectionDepth(Rectangle rectA, Rectangle rectB) {
            // Calculate half sizes.
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calculate centers.
            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // If we are not intersecting at all, return (0, 0).
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            // Calculate and return intersection depths.
            float depthX = distanceX == 0 ? 0 : distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX; //old: distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY == 0 ? 0 : distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY; //old: distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        //Convert an angle to a vector
        public static Vector2 AngleToVector(double angle) {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        //Convert degrees to radians
        public static double DegToRad(double angle) {
            return (Math.PI / 180) * angle;
        }

        //Calculate distance of two vectors
        public static int VectorDistance(Vector2 a, Vector2 b)
        {
            return (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));
        }
        #endregion
    }
}
