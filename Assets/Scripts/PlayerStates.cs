using UnityEngine;
using System.Collections;

public class PlayerStates : MonoBehaviour {
    

    public enum AnimationParameter
    {
        Idling=0,
        Running=1,
        Crouching=2,
        Falling=3,
        Stop=4,
        DodgingInAir = 5, 
        Jump=6,
        Landing=7,
        DodgeLanding=8
    }

    private static PlayerStates s_instance = null;
    [SerializeField]
    private GameObject _player;
    
    private Animator _playerAnimator = null;

    [SerializeField]
    private AnimationParameterInfo[] animatorParameters = null;
    
    public static void Set(AnimationParameter parameter)
    {
      
        s_instance.animatorParameters[(int)parameter].Set();
        
    }

    public static void UnSet(AnimationParameter parameter)
    {
        s_instance.animatorParameters[(int)parameter].UnSet();
    }

    public static void ResetTrigger(AnimationParameter parameter)
    {
        s_instance.animatorParameters[(int)parameter].ResetTrigger();
    }

    // Use this for initialization
    void Start () {
        s_instance = this;
        _playerAnimator = _player.GetComponent<Animator>();
        foreach(AnimationParameterInfo ap in animatorParameters)
        {
            ap.CalculateHash();
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDestroy()
    {
        s_instance = null;
    }
 

    [System.Serializable]
    public enum AnimParameterType { Bool, Trigger}

    [System.Serializable]
    public class AnimationParameterInfo
    {
        public string parameterName;
        public AnimParameterType parameterType;
        private int _parameterHash;
        public AnimationParameterInfo()
        {
            _parameterHash = Animator.StringToHash(parameterName);
        }
        public void CalculateHash()
        {
            _parameterHash = Animator.StringToHash(parameterName);
        }

        public void Set()
        {
            if(parameterType == AnimParameterType.Bool)
            {
                s_instance._playerAnimator.SetBool(_parameterHash, true);
            }
            else if (parameterType == AnimParameterType.Trigger)
            {
                s_instance._playerAnimator.SetTrigger(_parameterHash);
            }
        }

        public void UnSet()
        {
            if (parameterType == AnimParameterType.Bool)
            {
                s_instance._playerAnimator.SetBool(_parameterHash, false);
            }
            else if(parameterType == AnimParameterType.Trigger)
            {
                Debug.LogError("Cannot unset the trigger named>>" + parameterName);
            } 
        }

        public void ResetTrigger()
        {
            if (parameterType == AnimParameterType.Bool)
            {
                Debug.LogError("Cannot untrigger the bool named>>" + parameterName);                
            }
            else if (parameterType == AnimParameterType.Trigger)
            {
                s_instance._playerAnimator.ResetTrigger(_parameterHash);
            }
        }

    }
}
