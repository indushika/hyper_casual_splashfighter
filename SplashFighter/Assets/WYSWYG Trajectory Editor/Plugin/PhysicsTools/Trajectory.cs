using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
namespace PhysicsTools {
	[ExecuteInEditMode]
	public class Trajectory : MonoBehaviour {
		[System.Serializable]
		public enum LOGGING
		{
			NONE,
			INFO,
			DEBUG,
		}
		public bool loop = false;
		public List<WayPoint> lstWayPts;

		public int selectedWP;
		public bool ShowWayPoints;
		public LOGGING loggingType = LOGGING.INFO;
        public bool autoUpdate = true;
		List<Vector3> trajectory;
		Bezier bezier;
		List<Vector3> trajectoryOtherInfo;
		Bezier bezierOtherInfo;

		/// <summary>
		/// Debug the specified msg and type.
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <param name="type">Type.</param>
		public void Debug(String msg, LOGGING type)
		{
			if ((int)type <= (int)loggingType) {
				UnityEngine.Debug.Log(msg, this);
			}
		}
		public Bezier path(){
			return bezier;
		}
		public Bezier otherInfo(){
			return bezierOtherInfo;
		}
		public float TotalLength(){
			if (bezier != null) {
				return bezier.TotalLength ();
		
			}
			return  0;
		}
		public Iterator getIterator(Iterator.Direction dir, Iterator.Type type)
		{
			Iterator iter = new Iterator (this, dir, type);
			return iter;
		}

		void Awake(){
			Update ();
		}
		// Use this for initialization
		void Start () {
			
		}
    	// Update is called once per frame
		public void Update () {
            if (autoUpdate)
                refresh();
        }

		public void refresh(){
            if (lstWayPts != null && lstWayPts.Count >= 2)
            {
                trajectory = getTrajectoryGO();
                trajectoryOtherInfo = getTrajectoryOtherInfo();
                bezier = new Bezier(trajectory.ToArray(), loop);
                bezierOtherInfo = new Bezier(trajectoryOtherInfo.ToArray(), loop);
            }
        }
        public List<Vector3> getTrajectoryOtherInfo (){
			List<Vector3> lst = new List<Vector3> ();
			if (lstWayPts.Count > 0) {
				float twist = 0;
				float spd = 0;
				for (int i = 0; i < lstWayPts.Count; ++i) {
					lst.Add (new Vector3 (lstWayPts [i].twistValue, lstWayPts [i].speed, 0));
					twist = lstWayPts [i].twistValue;
					spd = lstWayPts [i].speed;
				}
			}
			return lst;
		}
		public List<Vector3> getTrajectoryGO(){
			List<Vector3> lst = new List<Vector3> ();
			if (lstWayPts != null) {
				for (int i = 0; i < lstWayPts.Count; ++i) {
					if (lstWayPts [i] != null)
						lst.Add (lstWayPts [i].position);
				}
			}
			return lst;
		}

		public void cleanUp()
		{
			List<GameObject> lstObj = new List<GameObject> ();
			for (int i = 0; i < transform.childCount; ++i) {
				if (transform.GetChild (i).name.Contains ("__wp_"))
					lstObj.Add (transform.GetChild (i).gameObject);
			}
			foreach (GameObject o in lstObj) {
				DestroyImmediate (o);
			}
			if(lstWayPts != null)
				lstWayPts.Clear ();
		}
		public void setWayPoints(List<WayPoint> lst){
			lstWayPts = new List<WayPoint> ();
			foreach (WayPoint pt in lst)
				lstWayPts.Add (pt);
			//lstWayPts = lst;
		}
		public void insertWayPt(int indexAfter)
		{
			if (indexAfter < 0)
				indexAfter = 0;
			else if (indexAfter > lstWayPts.Count - 1)
				indexAfter = lstWayPts.Count - 1;

			WayPoint pt = new WayPoint (this);
			if (indexAfter > 0) {
				Vector3 pos = (lstWayPts [indexAfter].position + lstWayPts [indexAfter - 1].position) / 2;
				pt.position = pos;	
			} else if (indexAfter == 0 && lstWayPts.Count > 1) {
				Vector3 pos = (3 * lstWayPts [indexAfter].position - lstWayPts [indexAfter + 1].position) / 2;
				pt.position = pos;	
			}
			lstWayPts.Insert (indexAfter, pt);
		}
		public bool removeWayPt(int index)
		{
			if (index >= 0 && index < lstWayPts.Count) {
				WayPoint pt = lstWayPts [index];
				DestroyImmediate(pt.pt);
				lstWayPts.RemoveAt (index);
				return true;
			}
			return false;
		}
		public void addWayPt()
		{
			if (lstWayPts == null) {
				lstWayPts = new List<WayPoint> ();
			}
			WayPoint pt = new WayPoint (this);
			if (lstWayPts.Count > 1) {
				WayPoint lastlastWP = lstWayPts [lstWayPts.Count - 2];
				WayPoint lastWP = lstWayPts [lstWayPts.Count - 1];
				Vector3 dir = (lastWP.position - lastlastWP.position).normalized;
				pt.position = lastWP.position + dir * 20;
				pt.pt.transform.localScale = lastWP.pt.transform.localScale;
				pt.pt.transform.localRotation = lastWP.pt.transform.localRotation;
			} else if (lstWayPts.Count > 0) {
				WayPoint lastWP = lstWayPts [lstWayPts.Count - 1];
				Vector3 dir = new Vector3(1, 0, 0);
				pt.position = lastWP.position + dir * 20;
				pt.pt.transform.localScale = lastWP.pt.transform.localScale;
				pt.pt.transform.localRotation = lastWP.pt.transform.localRotation;
			}

			lstWayPts.Add (pt);
		}
        public WayPoint addWayPt2(Vector3 position)
        {
            if (lstWayPts == null)
            {
                lstWayPts = new List<WayPoint>();
            }
            WayPoint pt = new WayPoint(this);
            if (lstWayPts.Count > 1)
            {
                WayPoint lastlastWP = lstWayPts[lstWayPts.Count - 2];
                WayPoint lastWP = lstWayPts[lstWayPts.Count - 1];
                Vector3 dir = (lastWP.position - lastlastWP.position).normalized;
                //TODO: pt.position = lastWP.position + dir * 20;
                //TODO:
                //pt.pt.transform.localScale = lastWP.pt.transform.localScale;
                pt.pt.transform.localRotation = lastWP.pt.transform.localRotation;
            }
            else if (lstWayPts.Count > 0)
            {
                WayPoint lastWP = lstWayPts[lstWayPts.Count - 1];
                Vector3 dir = new Vector3(1, 0, 0);
                pt.position = lastWP.position + dir * 20;
                //TODO: 
                //pt.pt.transform.localScale = lastWP.pt.transform.localScale;
                pt.pt.transform.localRotation = lastWP.pt.transform.localRotation;
            }
            pt.pt.transform.localScale = Vector3.one * 4.5f; 
            pt.position = position;
            pt.pt.tag = "Icing";
            lstWayPts.Add(pt);
            return pt;
        }
        public void showWaypoints(bool bShow)
		{
            ShowWayPoints = bShow;
            if (lstWayPts != null)
            {
                foreach (WayPoint pt in lstWayPts)
                {
                    if (pt != null && pt.pt != null)
                    {
                        pt.pt.GetComponent<MeshRenderer>().enabled = bShow;
                    }
                }
            }
		}
	}
	[System.Serializable]
	public class WayPoint
	{
		public Vector3 pos;
		public float spd;
		public float twist;
		private string name;
		public WayPoint(Trajectory traj){
			pt = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//pt.GetComponent<MeshRenderer> ().enabled = traj.ShowWayPoints;
            //TODO: CHANGED
            pt.GetComponent<MeshRenderer>().enabled = false;

            pt.name = "__wp_";
			//EditorGUIUtility.ShowObjectPicker

            //TODO: UNCOMMENT?
			//pt.transform.parent = traj.transform;
		}
		public string Name {
			get{ return name; }
			set{
				name = value;
				pt.name = "__wp_" + name;
			}
		}
		public Vector3 position
		{
			get{ return pt != null ? pt.transform.position : new Vector3(0, 0, 0); }
			set{ if(pt != null)
				pt.transform.position = value; }
		}
		public float speed
		{
			get{ return pt != null ? pt.transform.localScale.y : 1; }
			set {
				if (pt != null) {
					Vector3 v = pt.transform.localScale;
					v.y = value;
					pt.transform.localScale = v;
				}
			}
		}
		public float twistValue {
			get{ return pt != null ? pt.transform.rotation.eulerAngles.z : 0; }
			set {
				if (pt != null) {
					Vector3 v = pt.transform.rotation.eulerAngles;
					v.z = value;
					Quaternion q = Quaternion.Euler (v);
					pt.transform.rotation = q;
				}
			}
		}
		public void setPoint(GameObject o){
			pt = o;
		}

		[SerializeField]
		public GameObject pt;
	}
	[System.Serializable]
	public class TPoint
	{
		public Vector3 pt;
		public Vector3 dir;
		public Vector3 ypr;
		public float speed;
		public float radius;
	}
}