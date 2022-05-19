using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
namespace PhysicsTools{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Trajectory))]
	public class TrajectoryDrawer : MonoBehaviour{
		[System.Serializable]
		public enum TemplateType
		{
			CIRCLE,
			RECTANGLE,
			OPENRECTANGLE,
			TRAJECTORY,
		}
		public TemplateType templateType;
		public Trajectory template;
		public float radius = 0.1f;	//circle template
		public float rectangleWidth = 2.0f;
		public float rectangleHeight = 1;
		/// <summary>
		/// The length of the texture to be repeated 
		/// </summary>
		public float textureLength = 0.33f;
		[Range(1.0f, 100.0f)]
		public int smoothness = 2;
		public bool flipNormal = false;

		Mesh mesh = null;
		Vector3[] vertexBuffer = null;
		Vector2[] uv = null;
		int[] indexBuffer = null;
		Vector4[] tangents = null;

		public Trajectory thisTraj;

		void Awake(){
			Update ();
		}
		// Use this for initialization
		void Start () {
			Update ();
		}
		// Update is called once per frame
		public void Update () {
			thisTraj = gameObject.GetComponent<Trajectory> ();
			generateOverallMesh ();
		}
		void generateMeshForTrajectory(List<Vector3> trajectory, Bezier interpolator, float totalLen, 
			List<Vector3> templatePts, List<Vector3> templateTangents, 
			bool closed, 
			ref float lengthCovered, ref int lastVertId, ref int lastIndexID)
		{
            if (thisTraj.path() == null)
                return;
			int numTemplatePts = templatePts.Count;

			int vertId = lastVertId;
			Vector3 dir = new Vector3(0, 0, 1);
			Vector3 scale = new Vector3(1, 1, 1);
			float length = 0;
			Vector3 prevPos = trajectory[0];
			int numPts = trajectory.Count * smoothness;
			float delta = totalLen / (numPts - 1);
			Iterator iter = thisTraj.getIterator (Iterator.Direction.FORWARD, Iterator.Type.UNI);
			int s = 0, e = 0;
			float f =0;
			for (int i = 0; i < numPts; ++i)
			{
				if (thisTraj.loop && i == numPts - 1)// && smoothness != 1)
					break;
				if (thisTraj.lstWayPts != null && i / smoothness < thisTraj.lstWayPts.Count) {
					int idx = i / smoothness;
				}
				Vector3 res;
				if (i == 0) {
					res = iter.next (0, ref s, ref e, ref f);
					dir = iter.next (delta / totalLen) - res;
					iter.next (-delta / totalLen);
				} else
					res = iter.next (delta / totalLen, ref s, ref e, ref f);
				Vector3 otherInfo = thisTraj.otherInfo().point (s, e, f);
				float twist = otherInfo.x;
				if (i != 0)
				{
					dir = res - prevPos;
				}
				if (dir.sqrMagnitude == 0)
					dir.y = 1;
				dir.Normalize();
				Vector3 diff = res - prevPos;
				prevPos = res;

				//create transform at this point and rotation along the direction
				Matrix4x4 mat = new Matrix4x4();
				for (int k = 0; k < numTemplatePts; ++k)
				{
                    Quaternion q = Quaternion.LookRotation(-dir.normalized);
                    /*Vector3 newAxis = dir.normalized;
                    Vector3 cross = Vector3.Cross(newAxis, new Vector3(0, 1, 0));
                    float mag = cross.magnitude;
                    float angle = 0;
                    if (mag != 0)
                    {
                        cross.Normalize();
                        angle = (float)Math.Acos(Vector3.Dot(newAxis, new Vector3(0, 1, 0)));
                    }
                    Quaternion q = Quaternion.AngleAxis(angle * 180 / 3.14159f, -cross);
	                */				
                    Quaternion tq = Quaternion.AngleAxis (twist, new Vector3 (0, 0, 1));
					q = q * tq;
					mat.SetTRS(res, q, scale);

					//transform the circle points
					vertexBuffer[vertId] = transform.InverseTransformPoint(mat.MultiplyPoint(templatePts[k]));
					Vector3 t = transform.InverseTransformDirection(mat.MultiplyVector(templateTangents[k]));
					tangents[vertId] = new Vector4(t.x, t.y, t.z, -1);
					uv[vertId] = new Vector2(k * 1.0f / numTemplatePts, (totalLen - length) / textureLength);
					++vertId;
				}
				length += diff.magnitude;
			}
			bool bGenerateIndex = true;
			int totalVertex = vertexBuffer.Length;
			int templatePts1 = (closed ? numTemplatePts : numTemplatePts - 1);
			if (bGenerateIndex)
			{
				int cnt = lastIndexID;
				for (int k = 0; k < numPts - 1; ++k)
				{
					for (int l = 0; l < templatePts1; ++l)
					{
						int j = k + (lastVertId / numTemplatePts);
						indexBuffer[cnt++] = (j * numTemplatePts) + l;
						if (flipNormal) {
							indexBuffer [cnt++] = (j * numTemplatePts) + ((l + 1) % numTemplatePts);
							indexBuffer [cnt++] = (((j + 1) * numTemplatePts) + l) % totalVertex;
						} else {
							indexBuffer [cnt++] = (((j + 1) * numTemplatePts) + l) % totalVertex;
							indexBuffer [cnt++] = (j * numTemplatePts) + ((l + 1) % numTemplatePts);
						}
						indexBuffer[cnt++] = (j * numTemplatePts) + ((l + 1) % numTemplatePts);
						if (flipNormal) {
							indexBuffer [cnt++] = (((j + 1) * numTemplatePts) + ((l + 1) % numTemplatePts)) % totalVertex;
							indexBuffer [cnt++] = (((j + 1) * numTemplatePts) + l) % totalVertex;
						} else {
							indexBuffer [cnt++] = (((j + 1) * numTemplatePts) + l) % totalVertex;
							indexBuffer [cnt++] = (((j + 1) * numTemplatePts) + ((l + 1) % numTemplatePts)) % totalVertex;
						}
					}
				}
				lastIndexID = cnt;
			}
			lengthCovered += length;
			lastVertId = vertId;
		}
		void circleTemplate(float radius, out List<Vector3> pts, out List<Vector3> tangents, out bool closed)
		{
			int numCirclePt = 20;
			float angleDelta = 2 * 3.14159f / numCirclePt;
			pts = new List<Vector3>(numCirclePt);
			tangents = new List<Vector3>(numCirclePt);
			for (int j = 0; j < numCirclePt; ++j)
			{
				pts.Add(new Vector3((float)(radius * Math.Cos(j * angleDelta)), (float)(radius * Math.Sin(j * angleDelta)), 0));
				tangents.Add(new Vector3((float)(-radius * Math.Sin(j * angleDelta)), (float)(radius * Math.Cos(j * angleDelta)), 0));
				tangents[j].Normalize();
			}
			closed = true;
		}
		void openBoxTemplate(float length, float width, out List<Vector3> pts, out List<Vector3> tangents, out bool closed)
		{
			pts = new List<Vector3> ();
			tangents = new List<Vector3> ();
			pts.Add (new Vector3 (-length / 2, width, 0));
			pts.Add (new Vector3 (-length / 2, 0, 0));
			pts.Add (new Vector3 (length /  2, 0, 0));
			pts.Add (new Vector3 (length / 2, width, 0));
			tangents.Add (new Vector3 (0, -1, 0));
			tangents.Add (new Vector3 (1, 0, 0));
			tangents.Add (new Vector3 (0, 1, 0));
			tangents.Add (new Vector3 (0, 1, 0));
			closed = false;
		}
		void trajectoryTemplate(Trajectory traj, out List<Vector3> pts, out List<Vector3> tangents, out bool closed)
		{
			float numPts = traj.lstWayPts.Count * 3;
			pts = new List<Vector3> ();
			tangents = new List<Vector3> ();
			closed = traj.loop;
			Vector3 pt = new Vector3 (0, 0, 0);
			Vector3 tangent = new Vector3 (0, 0, 0);
			if (smoothness == 1) {
				pts.AddRange (traj.path ().knots);
				for (int i = 0; i < traj.path ().knots.Length; ++i) {
					if (i == 0) {
						pt = traj.path ().knots [i];
					} else {
						Vector3 nextPt = traj.path ().knots [i];
						tangent = nextPt - pt;
						tangents.Add (tangent.normalized);
						pt = nextPt;
					}
				}
			} else {
				Iterator iter = traj.getIterator (Iterator.Direction.FORWARD, Iterator.Type.UNI);
				for (int i = 0; i < numPts; ++i) {
					if (i == 0) {
						pt = iter.next (0);
					} else {
						Vector3 nextPt = iter.next (1.0f / (closed ? numPts : numPts - 1));
						tangent = nextPt - pt;
						tangents.Add (tangent.normalized);
						pt = nextPt;
					}
					pts.Add (pt);
				}
			}
			tangents.Add (tangent);
		}
		void generateOverallMesh()
		{
			List<Vector3> templatePts = null, templateTangents = null;
			bool closed = false;
			if(templateType == TemplateType.CIRCLE)
				circleTemplate (radius, out templatePts, out templateTangents, out closed);
			else if(templateType == TemplateType.RECTANGLE || templateType == TemplateType.OPENRECTANGLE)
				openBoxTemplate(rectangleWidth, rectangleHeight, out templatePts, out templateTangents, out closed);
			else if(template != null)
				trajectoryTemplate(template, out templatePts, out templateTangents, out closed);

			if (templatePts == null|| templatePts.Count == 0)
				return;
			thisTraj.Debug ("Generating Mesh", Trajectory.LOGGING.DEBUG);
			List<Vector3> trajectory = thisTraj.getTrajectoryGO();
			if (trajectory.Count <= 0)
				return;
			Bezier bezier = new Bezier (trajectory.ToArray (), thisTraj.loop);
			int numPts = 0;
			int numTris = 0;
			bool vertexCountIncreased = false;
			int ptCount = trajectory.Count * smoothness;
			numPts += ptCount;
			int numTemplatePts = templatePts.Count;
			if (closed)
				numTris += (ptCount - 1) * (numTemplatePts * 2);
			else
				numTris += (ptCount - 1) * ((numTemplatePts - 1) * 2);
			int vertCount = numTemplatePts * (thisTraj.loop ? numPts - 1: numPts);
			if (vertexBuffer == null || vertCount != vertexBuffer.Length) {
				if (vertexBuffer != null)
					vertexCountIncreased = (vertCount > vertexBuffer.Length);
				else
					vertexCountIncreased = true;
				vertexBuffer = new Vector3[vertCount];
				tangents = new Vector4[vertCount];
				uv = new Vector2[vertCount];
			}
			if (indexBuffer == null || numTris * 3 != indexBuffer.Length) {
				indexBuffer = new int[numTris * 3];
			}
			int lastVertId = 0;
			int lastIndexID = 0;
			float totalLen = bezier.TotalLength();

			float lengthCovered = 0;

			generateMeshForTrajectory (trajectory, bezier, totalLen, templatePts, templateTangents, closed, ref lengthCovered, ref lastVertId, ref lastIndexID);

			if (mesh == null)
				mesh = new Mesh();
			if (vertexCountIncreased) {
				mesh.vertices = vertexBuffer;
				mesh.triangles = indexBuffer;
			} else {
				mesh.triangles = indexBuffer;
				mesh.vertices = vertexBuffer;
			}
			mesh.uv = uv;
			mesh.RecalculateNormals();
			mesh.tangents = tangents;
			GetComponent<MeshFilter> ().sharedMesh = mesh;
			MeshCollider mc = GetComponent<MeshCollider> ();
			if(mc != null)
				mc.sharedMesh = mesh;
			//GetComponent<MeshRenderer> ().sharedMaterial = continuousMaterial;
			thisTraj.Debug ("Generating Mesh Done", Trajectory.LOGGING.DEBUG);
		}

	}
}