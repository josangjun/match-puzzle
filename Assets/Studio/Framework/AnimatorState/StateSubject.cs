using UnityEngine;

namespace Studio.Framework.AnimatorState
{
    [SharedBetweenAnimators]
    public class StateSubject : StateMachineBehaviour
    {
        [SerializeField]
        private string _label;
        public string label => _label;
        
        [SerializeField][HideInInspector]
        private int _labelHash = 0;
        public int labelHash
        { 
            get
            {
                if (_labelHash == 0)
                    _labelHash = Animator.StringToHash(_label);
                return _labelHash;
            }
        }

        private void OnValidate()
        {
            _labelHash = Animator.StringToHash(_label);
        }

        public AnimatorStateInfo info;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            this.info = stateInfo;
            var observer = animator.GetComponent<IStateObserver>();
            if (observer != null)
                observer.OnStateEnter(this);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            this.info = stateInfo;
            var observer = animator.GetComponent<IStateObserver>();
            if (observer != null)
                observer.OnStateExit(this);
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}