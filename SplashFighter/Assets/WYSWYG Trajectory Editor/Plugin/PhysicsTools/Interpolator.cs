using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace PhysicsTools
{
	/// <summary>
	/// Bezier curve implementation. creates a bezeir curve passing through the points specified
	/// </summary>
	public class Bezier
	{
		public float[] segmentLengths;
		public Vector3[] knots;
		public Vector3[] firstControlPts;
		public Vector3[] secondControlPts;
		int lastIndex = 1;
		float lengthCovered = 0;
		float indexedLength = 0;
		float totalLength = 0;
		public bool cyclic = false;
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicsTools.Bezier"/> class.
		/// </summary>
		/// <param name="points">Points. through which teh curve will pass through</param>
/*		public Bezier(List<PosOri> points)
		{
			Vector3 [] localknots = new Vector3[points.Count];
			for (int i = 0; i < points.Count; ++i) {
				localknots[i] = points[i].pos;
			}
			init (localknots);
		}
*/
		public Bezier(Vector3[] points, bool bCyclic = false){
			cyclic = bCyclic;
			init (points);
		}
		void init(Vector3[] points){
			knots = new Vector3[points.Length];
			for (int i = 0; i < points.Length; ++i) {
				knots [i] = points [i];
				//if(i != 0)
				//	totalLength += (knots[i] - knots[i - 1]).magnitude;
			}
			if (cyclic) {
				segmentLengths = new float[points.Length];
				//totalLength += (knots [knots.Length - 1] - knots [0]).magnitude;
			} else {
				segmentLengths = new float[points.Length - 1];
			}

			GetCurveControlPoints (knots, out firstControlPts, out secondControlPts);
			indexedLength = (knots[1] - knots[0]).magnitude;
			calculateLength ();
		}
		void calculateLength()
		{
			totalLength = 0;
			for (int s = 0; s < knots.Length; ++s) {
				if (!cyclic && s == knots.Length - 1) {
					break;
				}
				int e = (s + 1) % knots.Length;
				segmentLengths [s] = calculateSegmentLength (s, e);
				totalLength += segmentLengths [s];
			}
		}
		float calculateSegmentLength(int s, int e)
		{
			int n = 50;
			float df = 1.0f / n;
			float l = 0;
			Vector3 pt = knots [s];
			for (int i = 1; i <= n; ++i) {
				Vector3 curr = point (s, e, i * df);
				l += (curr - pt).magnitude;
				pt = curr;
			}
			return l;
		}
		/// <summary>
		/// total length of the curve.
		/// </summary>
		/// <returns>The length.</returns>
		public float TotalLength()
		{
			return totalLength;
		}
		public Vector3 point(int s, int e, float f){
			Vector3 pos = (float)(Math.Pow (1 - f, 3)) * knots [s] + 3 * (float)(Math.Pow (1 - f, 2)) * f * firstControlPts [s] + 3 * (1 - f) * f * f * secondControlPts [s] + f * f * f * knots [e];
			return pos;
		}
		/// <summary>
		/// Get the next point by passing the delta length
		/// </summary>
		/// <returns>The next.</returns>
		/// <param name="deltaLen">Delta length.</param>
		public float getInterval(ref int s, ref int e, float fraction)
		{
			s = e = 0;
			float fractionLength = fraction * totalLength;
			float lengthCovered = 0;
			int numPts = (cyclic ? knots.Length + 1 : knots.Length);
			float t = 0;
			for (int i = 0; i < numPts - 1; ++i) {
				int j = (i + 1) % knots.Length;
				float segLen = segmentLengths [i];
				lengthCovered += segLen;
				if (lengthCovered > fractionLength) {
					s = i;
					e = j;
					t = 1 - (lengthCovered - fractionLength) / segLen;
					break;
				}
			}
			if (s == e) {
				s = numPts - 2;
				e = (numPts - 1) % knots.Length;
				t = 1;
			}
			return t;
		}

		public Vector3 getNextPos(float deltaLen)
		{
			bool done = false;
			float secondLength = indexedLength;
			float firstLength = indexedLength;
			while (!done) {
				secondLength = indexedLength;
				firstLength = indexedLength - (knots[lastIndex] - knots[lastIndex - 1]).magnitude;
				if(lengthCovered + deltaLen > secondLength)
				{
					++lastIndex;
				}
				else
					break;

				if(lastIndex == knots.Length)
				{
					done = true;
					deltaLen = secondLength - lengthCovered;
					lastIndex = knots.Length - 1;
				}
				else{
					indexedLength += (knots[lastIndex] - knots[lastIndex - 1]).magnitude;
				}
			}
			float f = (lengthCovered + deltaLen - firstLength) / (secondLength - firstLength);
			Vector3 pos = (float)(Math.Pow (1 - f, 3)) * knots [lastIndex - 1] + 3 * (float)(Math.Pow (1 - f, 2)) * f * firstControlPts [lastIndex - 1] + 3 * (1 - f) * f * f * secondControlPts [lastIndex - 1] + f * f * f * knots [lastIndex];
			lengthCovered += deltaLen;
			return pos;
		}
		void GetCurveControlPoints(Vector3[] knots, out Vector3[] firstControlPoints, out Vector3[] secondControlPoints)
		{
			if (knots == null)
				throw new ArgumentNullException("knots");
			int n = knots.Length - 1;
			if (n < 1)
				throw new ArgumentException("At least two knot points required", "knots");
			if (n == 1)
			{ // Special case: Bezier curve should be a straight line.
				firstControlPoints = new Vector3[1];
				// 3P1 = 2P0 + P3
				firstControlPoints[0].x = (2 * knots[0].x + knots[1].x) / 3;
				firstControlPoints[0].y = (2 * knots[0].y + knots[1].y) / 3;
				firstControlPoints[0].z = (2 * knots[0].z + knots[1].z) / 3;

				secondControlPoints = new Vector3[1];
				// P2 = 2P1 – P0
				secondControlPoints[0].x = 2 * firstControlPoints[0].x - knots[0].x;
				secondControlPoints[0].y = 2 * firstControlPoints[0].y - knots[0].y;
				secondControlPoints[0].z = 2 * firstControlPoints[0].z - knots[0].z;
				return;
			}

			if (!cyclic) {
				// Calculate first Bezier control points
				// Right hand side vector
				float[] rhs = new float[n];

				// Set right hand side X values
				for (int i = 1; i < n - 1; ++i)
					rhs [i] = 4 * knots [i].x + 2 * knots [i + 1].x;
				rhs [0] = knots [0].x + 2 * knots [1].x;
				rhs [n - 1] = (8 * knots [n - 1].x + knots [n].x) / 2.0f;
	
				// Get first control points X-values
				float[] x = GetFirstControlPoints (rhs);

				// Set right hand side Y values
				for (int i = 1; i < n - 1; ++i)
					rhs [i] = 4 * knots [i].y + 2 * knots [i + 1].y;
				rhs [0] = knots [0].y + 2 * knots [1].y;
				rhs [n - 1] = (8 * knots [n - 1].y + knots [n].y) / 2.0f;

				// Get first control points Y-values
				float[] y = GetFirstControlPoints (rhs);

				// Set right hand side Z values
				for (int i = 1; i < n - 1; ++i)
					rhs [i] = 4 * knots [i].z + 2 * knots [i + 1].z;
				rhs [0] = knots [0].z + 2 * knots [1].z;
				rhs [n - 1] = (8 * knots [n - 1].z + knots [n].z) / 2.0f;
		
				// Get first control points Y-values
				float[] z = GetFirstControlPoints (rhs);

				// Fill output arrays.
				firstControlPoints = new Vector3[n];
				secondControlPoints = new Vector3[n];
				for (int i = 0; i < n; ++i) {
					// First control point
					firstControlPoints [i] = new Vector3 (x [i], y [i], z [i]);
					// Second control point
					if (i < n - 1)
						secondControlPoints [i] = new Vector3 (2 * knots [i + 1].x - x [i + 1], 2 * knots [i + 1].y - y [i + 1], 2 * knots [i + 1].z - z [i + 1]);
					else
						secondControlPoints [i] = new Vector3 ((knots [n].x + x [n - 1]) / 2, (knots [n].y + y [n - 1]) / 2, (knots [n].z + z [n - 1]) / 2);
				}
			} else {
				n += 1;
				// The matrix.
				float[] a = new float[n], b = new float[n], c = new float[n];
				for (int i = 0; i < n; ++i)
				{
					a[i] = 1;
					b[i] = 4;
					c[i] = 1;
				}

				// Right hand side vector for points X coordinates.
				float[] rhs = new float[n];
				for (int i = 0; i < n; ++i)
				{
					int j = (i == n - 1) ? 0 : i + 1;
					rhs[i] = 4 * knots[i].x + 2 * knots[j].x;
				}
				// Solve the system for X.
				float[] x = Cyclic.Solve(a, b, c, 1, 1, rhs);

				// Right hand side vector for points Y coordinates.
				for (int i = 0; i < n; ++i)
				{
					int j = (i == n - 1) ? 0 : i + 1;
					rhs[i] = 4 * knots[i].y + 2 * knots[j].y;
				}
				// Solve the system for Y.
				float[] y = Cyclic.Solve(a, b, c, 1, 1, rhs);

				// Right hand side vector for points Z coordinates.
				for (int i = 0; i < n; ++i)
				{
					int j = (i == n - 1) ? 0 : i + 1;
					rhs[i] = 4 * knots[i].z + 2 * knots[j].z;
				}
				// Solve the system for Z.
				float[] z = Cyclic.Solve(a, b, c, 1, 1, rhs);

				// Fill output arrays.
				firstControlPoints = new Vector3[n];
				secondControlPoints = new Vector3[n];
				for (int i = 0; i < n; ++i)
				{
					// First control point.
					firstControlPoints[i] = new Vector3(x[i], y[i], z[i]);
					int j = (i == 0 ? n - 1 : i - 1);
					// Second control point.
					secondControlPoints[j] = new Vector3(2 * knots[i].x - x[i], 2 * knots[i].y - y[i], 2 * knots[i].z - z[i]);
				}
			}
		}

		/// <summary>
		/// Solves a tridiagonal system for one of coordinates (x or y) of first Bezier control points.
		/// </summary>
		/// <param name="rhs">Right hand side vector.</param>
		/// <returns>Solution vector.</returns>
		float[] GetFirstControlPoints(float[] rhs)
		{
			int n = rhs.Length;
			float[] x = new float[n]; // Solution vector.
			float[] tmp = new float[n]; // Temp workspace.

			float b = 2.0f;
			x[0] = rhs[0] / b;
			for (int i = 1; i < n; i++) // Decomposition and forward substitution.
			{
				tmp[i] = 1 / b;
				b = (i < n - 1 ? 4.0f : 3.5f) - tmp[i];
				x[i] = (rhs[i] - x[i - 1]) / b;
			}
			for (int i = 1; i < n; i++)
				x[n - i - 1] -= tmp[n - i] * x[n - i]; // Backsubstitution.

			return x;
		}
	}

	/// <summary>
	/// Solves the cyclic set of linear equations.
	/// </summary>
	/// <remarks>
	/// The cyclic set of equations have the form
	/// ---------------------------
	/// b0 c0  0 · · · · · · ß
	///	a1 b1 c1 · · · · · · ·
	///  · · · · · · · · · · · 
	///  · · · a[n-2] b[n-2] c[n-2]
	/// a  · · · · 0  a[n-1] b[n-1]
	/// ---------------------------
	/// This is a tridiagonal system, except for the matrix elements 
	/// a and ß in the corners.
	/// </remarks>
	public static class Cyclic
	{
		/// <summary>
		/// Solves the cyclic set of linear equations. 
		/// </summary>
		/// <remarks>
		/// All vectors have size of n although some elements are not used.
		/// The input is not modified.
		/// </remarks>
		/// <param name="a">Lower diagonal vector of size n; a[0] not used.</param>
		/// <param name="b">Main diagonal vector of size n.</param>
		/// <param name="c">Upper diagonal vector of size n; c[n-1] not used.</param>
		/// <param name="alpha">Bottom-left corner value.</param>
		/// <param name="beta">Top-right corner value.</param>
		/// <param name="rhs">Right hand side vector.</param>
		/// <returns>The solution vector of size n.</returns>
		public static float[] Solve(float[] a, float[] b, 
			float[] c, float alpha, float beta, float[] rhs)
		{
			// a, b, c and rhs vectors must have the same size.
			if (a.Length != b.Length || c.Length != b.Length || 
				rhs.Length != b.Length)
				throw new ArgumentException
				("Diagonal and rhs vectors must have the same size.");
			int n = b.Length;
			if (n <= 2) 
				throw new ArgumentException
				("n too small in Cyclic; must be greater than 2.");

			float gamma = -b[0]; // Avoid subtraction error in forming bb[0].
			// Set up the diagonal of the modified tridiagonal system.
			float[] bb = new float[n];
			bb[0] = b[0] - gamma;
			bb[n-1] = b[n - 1] - alpha * beta / gamma;
			for (int i = 1; i < n - 1; ++i)
				bb[i] = b[i];
			// Solve A · x = rhs.
			float[] solution = Tridiagonal.Solve(a, bb, c, rhs);
			float[] x = new float[n];
			for (int k = 0; k < n; ++k)
				x[k] = solution[k];

			// Set up the vector u.
			float[] u = new float[n];
			u[0] = gamma;
			u[n-1] = alpha;
			for (int i = 1; i < n - 1; ++i) 
				u[i] = 0.0f;
			// Solve A · z = u.
			solution = Tridiagonal.Solve(a, bb, c, u);
			float[] z = new float[n];
			for (int k = 0; k < n; ++k)
				z[k] = solution[k];

			// Form v · x/(1 + v · z).
			float fact = (x[0] + beta * x[n - 1] / gamma)
				/ (1.0f + z[0] + beta * z[n - 1] / gamma);

			// Now get the solution vector x.
			for (int i = 0; i < n; ++i) 
				x[i] -= fact * z[i];
			return x;
		} 
	}
	/// <summary>
	/// Tridiagonal system solution.
	/// </summary>
	public static class Tridiagonal
	{
		/// <summary>
		/// Solves a tridiagonal system.
		/// </summary>
		/// <remarks>
		/// All vectors have size of n although some elements are not used.
		/// </remarks>
		/// <param name="a">Lower diagonal vector; a[0] not used.</param>
		/// <param name="b">Main diagonal vector.</param>
		/// <param name="c">Upper diagonal vector; c[n-1] not used.</param>
		/// <param name="rhs">Right hand side vector</param>
		/// <returns>system solution vector</returns>
		public static float[] Solve(float[] a, float[] b, float[] c, float[] rhs)
		{
			// a, b, c and rhs vectors must have the same size.
			if (a.Length != b.Length || c.Length != b.Length || 
				rhs.Length != b.Length)
				throw new ArgumentException
				("Diagonal and rhs vectors must have the same size.");
			if (b[0] == 0.0)
				throw new InvalidOperationException("Singular matrix.");
			// If this happens then you should rewrite your equations as a set of 
			// order N - 1, with u2 trivially eliminated.

			ulong n = Convert.ToUInt64(rhs.Length);
			float[] u = new float[n];
			float[] gam = new float[n]; 	// One vector of workspace, 
			// gam is needed.

			float bet = b[0];
			u[0] = rhs[0] / bet;
			for (ulong j = 1;j < n;j++) // Decomposition and forward substitution.
			{
				gam[j] = c[j-1] / bet;
				bet = b[j] - a[j] * gam[j];
				if (bet == 0.0)  
					// Algorithm fails.
					throw new InvalidOperationException
					("Singular matrix.");
				u[j] = (rhs[j] - a[j] * u[j - 1]) / bet;
			}
			for (ulong j = 1;j < n;j++) 
				u[n - j - 1] -= gam[n - j] * u[n - j]; // Backsubstitution.

			return u;
		}
	}
}