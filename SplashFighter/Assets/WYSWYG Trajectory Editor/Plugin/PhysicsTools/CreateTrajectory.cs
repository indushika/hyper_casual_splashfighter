using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace PhysicsTools{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Trajectory))]
	public class CreateTrajectory : MonoBehaviour {

		// Use this for initialization
		public int numWayPts = 10;

		public enum Type
		{
			LINE,
			CIRCLE,
			ELLIPTICAL,
			SQUARE,
			RECTANGULAR,
			HELICAL,
		}
		public Type type;
		//for circle/elliptical/square/rectangular trajectory
		public Vector3 center = new Vector3(0, 0, 0);
		public Vector3 axis = new Vector3(0, 1, 0);

		//for line
		public Vector3 startPt = new Vector3(0, 0, 0);
		public Vector3 endPt = new Vector3(0, 0, 10);

		//for circle
		public float radius = 10;
		//for ellipse
		public float majorAxis = 20;
		public float minorAxis = 10;

		//for square
		public float sideLength = 10;
		//for rectangle
		public float length = 10;
		public float width = 5;

		//for helix
		public float pitch = 2;
		public float numRounds = 4;

		public float fraction = 1;
		void Start () {
			type = Type.CIRCLE;
			center = new Vector3 (0, 0, 0);
			axis = new Vector3 (0, 1, 0);
			radius = 10;
			majorAxis = 20;
			minorAxis = 10;
			sideLength = 10;
			length = 20;
			width = 10;
			numWayPts = 10;
		}
		
		// Update is called once per frame
		void Update () {
		
		}
		public List<Vector3> generateHelicalPoints (float radius, float pitch, float numRounds, int num)
		{
			List<Vector3> lst = new List<Vector3> (num);
			float angleDelta = (float)(numRounds * 2.0f * Math.PI / num);
			for (int i = 0; i < num; ++i) {
				float x = (float)(radius * Math.Cos (i * angleDelta));
				float z = (float)(radius * Math.Sin (i * angleDelta));
				float y = (float)(i * angleDelta * pitch / (2 * Math.PI));
				lst.Add (new Vector3 (x, y, z));
			}
			return lst;
		}
		public List<Vector3> generateLinePoints(Vector3 start, Vector3 end, int num)
		{
			Vector3 dir = end - start;
			float length = dir.magnitude;
			float frac = length / (num - 1);
			dir.Normalize ();
			Vector3 pt = start;
			List<Vector3> lst = new List<Vector3> (num);
			for (int i = 0; i < num; ++i) {
				pt = i * frac * dir;
				lst.Add (pt);
			}
			return lst;
		}
		public List<Vector3> generateCirclePoints (float radius, int num, float fraction){
			return generateEllipsePoints (radius, radius, num, fraction);
		}
		public List<Vector3> generateEllipsePoints (float majorAxis, float minorAxis, int num, float fraction){
			float angleDelta = 2 * 3.14159f * fraction / num;
			List<Vector3> lst = new List<Vector3>(num);
			for (int i = 0; i < num; ++i)
			{
				lst.Add(new Vector3((float)(majorAxis * Math.Cos(i * angleDelta)), 0, (float)(minorAxis * Math.Sin(i * angleDelta))));
			}
			return lst;
		}
		public List<Vector3> generateSquarePoints (float side, int num){
			return generateRectanglePoints (side, side, num);
		}
		public List<Vector3> generateRectanglePoints (float l, float w, int num){
			Vector3[] pt = new Vector3[4];
			pt [0] = new Vector3 (-l / 2, 0, -w / 2);
			pt [1] = new Vector3 (l / 2, 0, -w / 2);
			pt [2] = new Vector3 (l / 2, 0, w / 2);
			pt [3] = new Vector3 (-l / 2, 0, w / 2);
			float perimeter = 2 * (l + w);
			float delta = perimeter / num;
			Vector3[] sides = new Vector3[4];
			float[] sideLengths = new float[4];
			List<Vector3> lst = new List<Vector3> ();
			for (int i = 0; i < 4; ++i) {
				sides [i] = pt [(i + 1) % 4] - pt [i];
				sideLengths[i] = sides[i].magnitude;
				sides[i].Normalize();
				lst.Add (pt [i]);
				for (int j = 0; j < num; ++j) {
					Vector3 point = pt [i] + (j + 1) * sideLengths [i] / (num + 1) * sides [i];
					lst.Add (point);
				}
			}
			/*int currSide = 0;
			List<Vector3> lst = new List<Vector3> (num);
			float lengthCovered = 0;
			Vector3 currPt = pt [0];
			for (int j = 0; j <= num; ++j) {
				Vector3 newPt = currPt + sides [currSide] * delta;
				lengthCovered += delta;
				lst.Add (newPt);
			}*/
			return lst;
		}
		public bool isLoop()
		{
			if (type == Type.LINE || type == Type.HELICAL)
				return false;
			else if ((type == Type.CIRCLE || type == Type.ELLIPTICAL) && fraction != 1)
				return false;
			else
				return true;
		}
		public List<Vector3> generatePoints(){
			Matrix4x4 transform = new Matrix4x4 ();
            Vector3 newAxis = axis.normalized;
            Vector3 cross = Vector3.Cross(newAxis, new Vector3(0, 1, 0));
            float mag = cross.magnitude;
            float angle = 0;
            if(mag != 0)
            {
                cross.Normalize();
                angle = (float)Math.Acos(Vector3.Dot(newAxis, new Vector3(0, 1, 0)));
            }
            Quaternion q = Quaternion.AngleAxis(angle * 180 / 3.14159f, cross);
			transform.SetTRS (center, q, new Vector3 (1, 1, 1));
			List<Vector3> pts = new List<Vector3> ();
			switch (type) {
			case Type.LINE:
				{
					pts = generateLinePoints (startPt, endPt, numWayPts);
					break;
				}
			case Type.CIRCLE:
				{
					pts = generateCirclePoints (radius, numWayPts, fraction);
					break;
				}
			case Type.ELLIPTICAL:
				{
					pts = generateEllipsePoints (majorAxis, minorAxis, numWayPts,  fraction);
					break;
				}
			case Type.SQUARE:
				{
					pts = generateSquarePoints (sideLength, numWayPts);
					break;
				}
			case Type.RECTANGULAR:
				{
					pts = generateRectanglePoints (length, width, numWayPts);
					break;
				}
			case Type.HELICAL:
				{
					pts = generateHelicalPoints (radius, pitch, numRounds, numWayPts);
					break;
				}
			}
			if (type != Type.LINE) {
				for (int i = 0; i < pts.Count; ++i) {
					pts[i] = transform.MultiplyPoint(pts[i]);
				}
			}
			return pts;
		}
		public void generateTrajectory()
		{
			if (gameObject) {
				Trajectory trajectory = gameObject.GetComponent<Trajectory> ();
				trajectory.cleanUp ();
				trajectory.loop = isLoop();
				//trajectory.s = "hello";
				List<WayPoint> lstWayPts = new List<WayPoint> ();
				List<Vector3> lstPts = generatePoints ();
				for (int i = 0; i < lstPts.Count; ++i) {
					lstWayPts.Add (new WayPoint (trajectory));
					lstWayPts [i].pt.transform.localPosition = lstPts [i];
				}
				trajectory.setWayPoints (lstWayPts);
                trajectory.refresh();
			}
		}
	}
}