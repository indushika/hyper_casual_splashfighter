using UnityEngine;
using System.Collections;

namespace PhysicsTools{
public class MoveOnTrajectory : MonoBehaviour {

	// Use this for initialization
	public Trajectory trajectory;
	public float speed = 1;
	public Vector3 rotationOffset;
	public Iterator.Type type = Iterator.Type.UNI;
	public Iterator.Direction dir = Iterator.Direction.FORWARD;
	public float fraction = 0;
	public float radius = 0;
	public float outputSpeed = 0;
    public bool applyRoll = true;
    public bool applyPitch = true;
    public bool applyHeading = true;
	Quaternion offset;
	Iterator.Type prevType = Iterator.Type.UNI;
	Iterator.Direction prevDir= Iterator.Direction.FORWARD;
	Iterator iter;
	float internalSpeed = 1;

	void Start () {
		iter = trajectory.getIterator (dir, type);
	}

	
	// Update is called once per frame
	void Update () {
		if(trajectory != null) {
			if (prevDir != dir || prevType != type) {
				iter = trajectory.getIterator (dir, type);
				prevDir = dir;
				prevType = type;
			}
			if (iter == null) {
				iter = trajectory.getIterator (dir, type);
			}
				if (internalSpeed != internalSpeed)
					internalSpeed = 1;
			offset.eulerAngles = rotationOffset;
			//fraction += (speed * Time.deltaTime) / trajectory.TotalLength ();
			float deltaFraction = speed * internalSpeed * Time.deltaTime / trajectory.TotalLength ();
			if (trajectory.path () == null)
				return;
			TPoint pt = iter.nextTPoint (deltaFraction);
			outputSpeed = (pt.pt - transform.position).magnitude  / Time.deltaTime;
			transform.position = pt.pt;
			radius = pt.radius;
			internalSpeed = pt.speed;
			transform.forward = applyHeading ? pt.dir : Vector3.forward;
			Quaternion delta = Quaternion.Euler (pt.ypr.y, applyPitch ? pt.ypr.x : 0.0f, applyRoll ? pt.ypr.z : 0.0f);
			transform.rotation *= delta;
			fraction = iter.fractionCovered;
		}
	}
}
}