using UnityEngine;

namespace Studio.Framework
{
	[ExecuteInEditMode]
	public class Billboard : MonoBehaviour {
		
		public enum Axis { None, X = 0x1, Y = 0x2, Z = 0x4 };

		public Axis _axis = Axis.None;
		public Axis axis { get { return _axis; } }

		public enum Space { Local, World }
		public Space _space = Space.World;

        [SerializeField]
        private Camera _target = null;
        public Camera target {
            get
            {
                if (!_target)
                    return Camera.main;
                return _target;
            }
            set
            {
                _target = value;
            }
        }
		
		// Update is called once per frame
		void LateUpdate()
		{
            if (!target) return;
			if (_space == Space.World)
			{
				var fwd = (target.transform.position - transform.position);
				if (axis == Axis.Y)
					fwd.y = 0f;
				else if (axis == Axis.X)
					fwd.x = 0f;
				else if (axis == Axis.Z)
					fwd.z = 0f;
				transform.rotation = Quaternion.LookRotation(fwd);
			}
			else
			{
				var fwd = (target.transform.position - transform.position);
				Vector3 up = transform.up;
				var parent = transform.parent;
				if (parent)
				{
					if (axis == Axis.Y)
						up = transform.parent.up;
					else if (axis == Axis.X)
						up = transform.parent.right;
					else if (axis == Axis.Z)
						up = transform.parent.forward;
				}
				var left = Vector3.Cross(up, fwd);
				fwd = Vector3.Cross(up, left);
				transform.rotation = Quaternion.LookRotation(fwd, up);
			}
		}
	}
}