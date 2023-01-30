using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Opencoding.Console.TouchDetectors
{
	internal class ThreeFingersHeldTouchDetector : TouchDetector
	{
		private readonly Dictionary<int, Vector2> _initialTouchPositions = new Dictionary<int, Vector2>();

		private bool _touchCancelled;

		private float _timeToActivateConsole = -1f;

		private const float TIME_TO_HOLD_FINGERS_BEFORE_ACTIVATING = 0.6f;

		private const float MAX_MOVEMENT_ALLOWED = 20f;

		private int touchCount
		{
			get
			{
				var count = 0;
                var touches = Touchscreen.current.touches;
                for (int i = 0; i < touches.Count; i++)
                {
                    var touch = touches[i];
                    var state = touch.ReadValue();

                    switch (state.phase)
                    {
                        case TouchPhase.Began:
                        case TouchPhase.Ended:
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
							count++;
                            break;
						default:
							return count;
                    }
                }
				return count;
            }
		}

		public bool Update()
		{
            var touches = Touchscreen.current.touches;
			for (int i = 0; i < touches.Count; i++)
			{
				var touch = touches[i];
				var state = touch.ReadValue();
				
				switch (state.phase)
				{
					case TouchPhase.Began:
                        HandleTouchBegan(touch, i);
                        break;
                    case TouchPhase.Canceled:
                        HandleTouchCanceled(touch);
                        break;
                    case TouchPhase.Ended:
                        HandleTouchEnded(touch);
                        break;
                    case TouchPhase.Moved:
                        if (_initialTouchPositions.ContainsKey(i))
						{
							HandleTouchMoved(touch, i);
							break;
						} else
						{
                            HandleTouchBegan(touch, i);
                        }
						break;
                    case TouchPhase.Stationary:
                        break;
                }
			}
			if (_timeToActivateConsole < 0f && !_touchCancelled)
			{
				if (touchCount == 3)
				{
					_timeToActivateConsole = Time.realtimeSinceStartup + 0.6f;
				}
				else
				{
					_timeToActivateConsole = -1f;
				}
			}
			else if (_timeToActivateConsole > 0f && Time.realtimeSinceStartup > _timeToActivateConsole && !_touchCancelled)
			{
				return true;
			}
			return false;
		}

		private void HandleTouchBegan(TouchControl touch, int fingerId)
		{
			if (fingerId == 0)
			{
				_touchCancelled = false;
				_timeToActivateConsole = -1f;
			}
			_initialTouchPositions[fingerId] = touch.position.ReadValue();
		}

		private void HandleTouchMoved(TouchControl touch, int fingerId)
		{
			Vector2 val = _initialTouchPositions[fingerId];
			if (Vector2.Distance(touch.position.ReadValue(), val) > 20f)
			{
				_touchCancelled = true;
			}
        }

		private void HandleTouchCanceled(TouchControl touch)
		{
			_touchCancelled = true;
        }

		private void HandleTouchEnded(TouchControl touch)
		{
			_touchCancelled = true;
        }
	}
}
