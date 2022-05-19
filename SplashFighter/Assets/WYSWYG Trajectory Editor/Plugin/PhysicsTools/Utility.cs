using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PhysicsTools
{
	/// <summary>
	/// Utility class for some commonly used function
	/// </summary>
	public class Utility
	{
		/// <summary>
		/// Conversion factor from degree to radian
		/// </summary>
		const float DEG_2_RAD = 0.017453278f;
		/// <summary>
		/// Conversion factor from radian to degree
		/// </summary>
		const float RAD_2_DEG = 57.29582791f;

		/// <summary>
		/// Convert Matrix to translation, rotation and scale to be used in transform
		/// </summary>
		/// <param name="mat">Mat.</param>
		/// <param name="pos">Position.</param>
		/// <param name="rot">Rot.</param>
		/// <param name="scale">Scale.</param>
		public static void MatrixToTRS(Matrix4x4 mat, out Vector3 pos, out Quaternion rot, out Vector3 scale)
		{
			// Extract new local position
			pos = mat.GetColumn(3);

			// Extract new local rotation
			rot = Quaternion.LookRotation(
				mat.GetColumn(2),
				mat.GetColumn(1)
			);

			// Extract new local scale
			scale = new Vector3(
				mat.GetColumn(0).magnitude,
				mat.GetColumn(1).magnitude,
				mat.GetColumn(2).magnitude
			);
		}
		/// <summary>
		/// Swap the specified lhs and rhs.
		/// </summary>
		/// <param name="lhs">Lhs.</param>
		/// <param name="rhs">Rhs.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}
		/// <summary>
		/// Gets the catenary pts.
		/// </summary>
		/// <returns>The catenary pts.</returns>
		/// <param name="vStart">Start Point</param>
		/// <param name="vEnd">End Point</param>
		/// <param name="r_length">Total Length</param>
		/// <param name="N">Number of points in which the catenray will be divided</param>
		public static List<Vector3> getCatenaryPts(Vector3 vStart, Vector3 vEnd, float r_length, int N)
		{
			bool bReversed = false;
			int maxIter = 100;       //maximum number of iterations
			float minGrad = 1e-10f; //minimum norm of gradient
			float minVal = 1e-8f;  //minimum norm of sag function
			float stepDec = 0.5f;   //factor for decreasing stepsize
			float minStep = 1e-9f;	//minimum step size
			float minHoriz = 1e-3f;	//minimum horizontal distance
			if (vStart.y > vEnd.y)
			{
				Swap(ref vStart, ref vEnd);
				bReversed = true;
			}

			Vector3 vDiff = vEnd - vStart;
			float h = vDiff.y;
			vDiff.y = 0;
			Vector3 vDelta = vDiff / (N - 1);
			float d = vDiff.magnitude;
			vDiff.Normalize();
			float sag = (d != 0) ? 1 / d : 1.0f;
			List<Vector3> vecPts = new List<Vector3>(new Vector3[N]);

			if (Math.Abs(d) < minHoriz) //almost perfectly vertica)
			{
				Vector3 vPt = new Vector3((vStart.x + vEnd.x) / 2, 0, (vStart.z + vEnd.z) / 2);
				if (r_length < Math.Abs(h)) //rope is stretched
				{
					for (int i = 0; i < N; ++i)
					{
						vecPts[i] = vPt + new Vector3(0.0f, vStart.y + h * (i * 1.0f / (N - 1)), 0.0f);
					}
				}
				else
				{
					sag = (r_length - Math.Abs(h)) / 2;
					int n_sag = (int)Math.Ceiling(N * sag / r_length);
					float y_max = Math.Max(vStart.y, vEnd.y);
					float y_min = Math.Min(vStart.y, vEnd.y);
					for (int i = 0; i < N; ++i)
					{
						if (i < N - n_sag)
						{
							float diff = (y_min - sag - y_max);
							float delta = diff * (i * 1.0f / (N - n_sag - 1.0f));
							vecPts[i] = vPt + new Vector3(0.0f, y_max + delta, 0.0f);
						}
						else
						{
							float diff = sag;
							float delta = diff * (i * 1.0f / (n_sag - 1.0f));
							vecPts[i] = vPt += new Vector3(0.0f, y_min - sag + delta, 0.0f);
						}
					}
				}
			}
			else
			{
				if (r_length <= Math.Sqrt(d * d + h * h))	//rope is stretched : straight line
				{
					for (int i = 0; i < N; ++i)
					{
						Vector3 vPt = vStart + i * vDelta;
						float yDelta = i * (vEnd.y - vStart.y) / (N - 1);
						vPt.y = vStart.y + yDelta;
						vecPts[i] = vPt;
					}
				}
				else
				{
					Calc c = new Calc(r_length, h, d);

					for (int k = 0; k < maxIter; ++k)
					{
						float val = c.g(sag);
						float grad = c.dg(sag);
						if (Math.Abs(val) < minVal || Math.Abs(grad) < minGrad)
						{
							break;
						}
						float search = -c.g(sag) / c.dg(sag);

						float alpha = 1;
						float sag_new = sag + alpha * search;

						while (sag_new < 0 || Math.Abs(c.g(sag_new)) > Math.Abs(val))
						{
							alpha = stepDec * alpha;
							if (alpha < minStep)
							{
								break;
							}
							sag_new = sag + alpha * search;
						}
						sag = sag_new;
					}
					float x_left = (float)(0.5 * (Math.Log((r_length + h) / (r_length - h)) / sag - d));
					vDiff.y = 0; vDiff.Normalize();
					Vector3 x_min = vStart - x_left * vDiff; x_min.y = 0;
					float bias = (float)(vStart.y - Math.Cosh(x_left * sag) / sag);

					for (int i = 0; i < N; ++i)
					{
						Vector3 vPt = vStart + i * vDelta;
						Vector3 vLinePt = vPt;
						vPt.y = 0;
						vPt.y = (float)(Math.Cosh((vPt - x_min).magnitude * sag) / sag + bias);
						//float diff = vLinePt.y + (h / (N - 1) * i) - vPt.y;
						//vPt.y += 2 * diff;
						vecPts[i] = vPt;
					}
				}
			}
			if (bReversed)
				vecPts.Reverse();
			return vecPts;
		}
		public class Calc
		{
			public float g(float s)
			{
				float val = (float)(2.0f * Math.Sinh(s * d / 2.0f) / s - Math.Sqrt(r_length * r_length - h * h));
				return val;
			}
			public float dg(float s)
			{
				float val = (float)(2 * Math.Cosh(s * d / 2) * d / (2 * s) - 2 * Math.Sinh(s * d / 2) / (s * s));
				return val;
			}
			public Calc(float r_, float h_, float d_)
			{
				r_length = r_; h = h_; d = d_;
			}
			float r_length;
			float h;
			float d;
		};
		/// <summary>
		/// Creates the orientation quaternion from a vector
		/// </summary>
		/// <returns>The orientation.</returns>
		/// <param name="v">V.</param>
		public static Quaternion createOrientation(Vector3 v)
		{
			if (v.x == 0 && v.y == 0 && v.z == 0)
			{
				Quaternion q = Quaternion.identity;
				return q;
			}
			else
			{
				Vector3 up = new Vector3(0, 1, 0);
				Vector3 cross = Vector3.Cross(v, up);
				cross.Normalize();
				float dot = Vector3.Dot(up, v);
				if (dot > 1) dot = 1;
				else if (dot < -1) dot = -1;
				float angle = (float)Math.Acos(dot);
				Quaternion q = Quaternion.AngleAxis(angle * 180.0f / 3.14159f, -cross);
				return q;
			}
		}
		/// <summary>
		/// Create a deep copy of the object
		/// </summary>
		/// <returns>The clone.</returns>
		/// <param name="a">The alpha component.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T DeepClone<T>(T a)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, a);
				stream.Position = 0;
				return (T)formatter.Deserialize(stream);
			}
		}
		static Dictionary<UnityEngine.Rigidbody, List<UnityEngine.Joint>> buildMapOfJoints()
		{
			Dictionary<UnityEngine.Rigidbody, List<UnityEngine.Joint>> dic = new Dictionary<UnityEngine.Rigidbody, List<UnityEngine.Joint>> ();
			UnityEngine.Joint[] arr = GameObject.FindObjectsOfType<UnityEngine.Joint> ();
			foreach (UnityEngine.Joint jt in arr) {
				Rigidbody own = jt.gameObject.GetComponent<Rigidbody> ();
				if (own != null) {
					if (!dic.ContainsKey (own)){
						dic.Add(own, new List<UnityEngine.Joint>());
					}
					dic [own].Add (jt);
				}
				Rigidbody other = jt.connectedBody;
				if (other != null) {
					if (!dic.ContainsKey (other)){
						dic.Add(other, new List<UnityEngine.Joint>());
					}
					dic [other].Add (jt);
				}
			}
			return dic;
		}
		static void getConnectedBodies(ref Dictionary<UnityEngine.Rigidbody, List<UnityEngine.Joint>> dic, Rigidbody body, ref HashSet<Rigidbody> bodies)
		{
			bodies.Add (body);
			if (dic.ContainsKey (body)) {
				List<UnityEngine.Joint> joints = dic [body];

				foreach (UnityEngine.Joint jt in joints) {
					if (jt.connectedBody != null && !bodies.Contains (jt.connectedBody))
						getConnectedBodies (ref dic, jt.connectedBody, ref bodies);
					Rigidbody ownBody = jt.gameObject.GetComponent<Rigidbody> ();
					if (ownBody != null && !bodies.Contains (ownBody))
						getConnectedBodies (ref dic, ownBody, ref bodies);
				}
			}
		}
		public static void moveConnectedBodies(Rigidbody start, Matrix4x4 newPose)
		{
			Dictionary<UnityEngine.Rigidbody, List<UnityEngine.Joint>> dic = buildMapOfJoints ();

			HashSet<Rigidbody> movedBodies = new HashSet<Rigidbody> ();
			getConnectedBodies (ref dic, start, ref movedBodies);

			Matrix4x4 startPose = Matrix4x4.TRS(start.transform.position, start.transform.rotation, new Vector3(1, 1, 1));
			foreach (Rigidbody body in movedBodies) {
				Matrix4x4 currPose = Matrix4x4.TRS(body.transform.position, body.transform.rotation, new Vector3(1, 1,1 ));
				Matrix4x4 startPoseInv = startPose.inverse;
				currPose = startPoseInv * currPose;
				currPose = newPose * currPose;
				Vector3 pos, scale;
				Quaternion rot;
				MatrixToTRS (currPose, out pos, out rot, out scale);
				body.transform.position = pos;
				body.transform.rotation = rot;
			}
		}
	}
}
