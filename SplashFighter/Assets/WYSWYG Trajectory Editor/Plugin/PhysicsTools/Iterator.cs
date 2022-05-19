using System;
using UnityEngine;
namespace PhysicsTools
{
	public class Iterator
	{
		[System.Serializable]
		public enum Direction{
			FORWARD,
			BACKWARD,
		}
		[System.Serializable]
		public enum Type{
			UNI,
			CYCLE,
			OSCILLATION,
		}
		Trajectory traj;
		Direction enDir;
		Type enType;
		public float fractionCovered = 0;
		public Iterator(Trajectory traj, Direction enDir, Type enType){
			this.enDir = enDir;
			this.enType = enType;
			this.traj = traj;
		}
		float getFraction(float deltaFrac){
			float fraction = fractionCovered;
			fraction += deltaFrac;
			switch (enType) {
			case Type.UNI:
				{
					fraction = Math.Min (fraction, 1.0f);
					break;
				}
			case Type.CYCLE:
				{
					fraction = (fraction + 10000) % 1.0f;
					break;
				}
			case Type.OSCILLATION:
				{
					fraction = (fraction + 10000) % 2.0f;
					if (fraction > 1) {
						fraction = fraction - 1;
						enDir = (enDir == Direction.FORWARD ? Direction.BACKWARD : Direction.FORWARD);
					}
					break;
				}
			}
			fractionCovered = fraction;
			switch (enDir) {
			case Direction.BACKWARD:
				{
					fraction = 1 - fraction;
					break;
				}
			}
			return fraction;
		}
		public Vector3 next(float deltaFrac){
			float absoluteFraction = getFraction (deltaFrac);
			int s = 0, e = 0;
			Bezier bez = traj.path ();
			//float f = getInterval (ref s, ref e, absoluteFraction);
			float f = bez.getInterval(ref s, ref e, absoluteFraction);
			Vector3 pos = bez.point (s, e, f);
			return pos;
		}
		public Vector3 next(float deltaFrac, ref int s, ref int e, ref float f){
			float absoluteFraction = getFraction (deltaFrac);
			s = 0;
			e = 0;
			Bezier bez = traj.path ();
			//f = getInterval (ref s, ref e, absoluteFraction);
			f = bez.getInterval(ref s, ref e, absoluteFraction);
			Vector3 pos = bez.point(s, e, f);
			return pos;
		}
		public TPoint nextTPoint(float deltaFrac){
			float absoluteFraction = getFraction (deltaFrac);
			Bezier bez = traj.path ();
			int s = 0, e = 0;
			//float f = getInterval (ref s, ref e, absoluteFraction);
			float f = bez.getInterval(ref s, ref e, absoluteFraction);
			Vector3 pos = (float)(Math.Pow (1 - f, 3)) * bez.knots [s] + 3 * (float)(Math.Pow (1 - f, 2)) * f * bez.firstControlPts [s] + 3 * (1 - f) * f * f * bez.secondControlPts [s] + f * f * f * bez.knots [e];
			Vector3 slope = (float)(3 * Math.Pow (1 - f, 2) * -1) * bez.knots [s] + 3 * (float)(-2 * (1 - f) * f + (1 - f) * (1 - f)) * bez.firstControlPts [s] + (3 * (1 - f) * 2 * f - 3 * f * f) * bez.secondControlPts [s] + 3 * f * f * bez.knots [e];
			Vector3 slopeDer = (float)(6 * (1 - f)) * bez.knots [s] + 3 * (-(1 - 3 * f) - 3 * (1 - f)) * bez.firstControlPts [s] + (6 - 18 * f) * bez.secondControlPts [s] + 6 * f * bez.knots [e];
			float den = (float)Math.Sqrt (slope.sqrMagnitude * slopeDer.sqrMagnitude - Math.Pow (Vector3.Dot (slope, slopeDer), 2));
			float radius = float.MaxValue;
			if (den != 0) {
				radius = (float)Math.Pow (slope.magnitude, 3) / den;
			}
			//Quaternion q = Quaternion.LookRotation (slope.normalized, new Vector3 (0, 1, 0));
			Bezier otherInfo = traj.otherInfo();
			float prevSpeed = otherInfo.knots [s].y;
			float nextSpeed = otherInfo.knots [e].y;
			float speed = prevSpeed + (nextSpeed - prevSpeed) * f;
			float prevTwist = otherInfo.knots [s].z;
			float nextTwist = otherInfo.knots [e].z;

			Vector3 otherInfoPt = otherInfo.point (s, e, f);
			TPoint p = new TPoint();
			p.pt = pos;
			p.dir = slope.normalized;
			//float twist = traj.twistCurve.Evaluate (absoluteFraction * bez.TotalLength ());
			//p.ypr = new Vector3 (0, twist, 0); 
			p.ypr = new Vector3 (0, otherInfoPt.z, otherInfoPt.x);
			//p.speed = traj.speedCurve.Evaluate (absoluteFraction * bez.TotalLength ());
			p.speed = speed;//otherInfoPt.y;
			p.radius = radius;
			return p;
		}
		float getInterval(ref int s, ref int e, float fraction)
		{
			Bezier bez = traj.path ();
			s = e = 0;
			float fractionLength = fraction * bez.TotalLength ();
			float lengthCovered = 0;
			int numPts = (bez.cyclic ? bez.knots.Length + 1 : bez.knots.Length);
			float t = 0;
			for (int i = 0; i < numPts - 1; ++i) {
				int j = (i + 1) % bez.knots.Length;
				float segLen = (bez.knots [j] - bez.knots [i]).magnitude;
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
				e = (numPts - 1) % bez.knots.Length;
				t = 1;
			}
			return t;
		}
		public void reset(){
			switch (enDir) {
			case Direction.FORWARD:
				fractionCovered = 0;
				break;
			case Direction.BACKWARD:
				fractionCovered = 1;
				break;
			}
		}
	}
}

