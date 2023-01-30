using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Opencoding.Console.TouchDetectors
{
	internal class TwoFingerSwipeTouchDetector : TouchDetector
	{
		private readonly Dictionary<int, Vector2> _initialTouchPositions = new Dictionary<int, Vector2>();

		private readonly Dictionary<int, Vector2> _maximumOffsetPosition = new Dictionary<int, Vector2>();

		private const float DRAG_DISTANCE_REQUIRED = 180f;

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
			bool shouldShowConsole = false;
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
			{
				var touch = touches[i];
                var state = touch.ReadValue();
                var phase = state.phase;
                switch (phase)
				{
				case TouchPhase.Began:
					HandleTouchBegan(touch, i);
					break;
				case TouchPhase.Moved:
					if (_initialTouchPositions.ContainsKey(i))
					{
						HandleTouchMoved(touch, i);
						break;
					}
                        HandleTouchBegan(touch, i);
                        break;
				case TouchPhase.Ended:
					HandleTouchEnded(touch, i, ref shouldShowConsole);
					break;
				case TouchPhase.Canceled:
					HandleTouchCanceled(touch, i);
					break;
				}
			}
			return shouldShowConsole;
		}

		private void HandleTouchBegan(TouchControl touch, int fingerId)
		{
			_maximumOffsetPosition.Remove(fingerId);
			_initialTouchPositions[fingerId] = touch.position.ReadValue();
        }

		private void HandleTouchMoved(TouchControl touch, int fingerId)
		{
			Vector2 val = _initialTouchPositions[fingerId];
			if (!_maximumOffsetPosition.TryGetValue(fingerId, out var value))
			{
				value = val;
			}
			float num = Vector2.Distance(val, value);
			if (Vector2.Distance(val, touch.position.ReadValue()) > num)
			{
				_maximumOffsetPosition[fingerId] = touch.position.ReadValue();
			}
        }

		private void HandleTouchCanceled(TouchControl touch, int fingerId)
		{
			_initialTouchPositions.Remove(fingerId);
			_maximumOffsetPosition.Remove(fingerId);
        }

		private void HandleTouchEnded(TouchControl touch, int fingerId, ref bool shouldShowConsole)
		{
			if (_initialTouchPositions.ContainsKey(fingerId))
			{
				Vector2 val = _initialTouchPositions[fingerId];
				if (!_maximumOffsetPosition.TryGetValue(fingerId, out var value))
				{
					value = val;
				}
				Vector2 offset = value - val;
				float magnitude = offset.magnitude;
				DetectSwipeDown(magnitude, offset, ref shouldShowConsole);
				DetectSwipeUp(magnitude, offset);
				_initialTouchPositions.Remove(fingerId);
				_maximumOffsetPosition.Remove(fingerId);
			}
        }

		private void DetectSwipeUp(float maxDistanceMoved, Vector2 offset)
		{
			if (maxDistanceMoved > 180f && Vector2.Dot(offset.normalized, Vector2.up) > 0.85f && touchCount == 2)
			{
				DebugConsole.IsVisible = false;
			}
		}

		private void DetectSwipeDown(float maxDistanceMoved, Vector2 offset, ref bool shouldShowConsole)
		{
			if (maxDistanceMoved > 180f && Vector2.Dot(offset.normalized, -Vector2.up) > 0.85f && touchCount == 2)
			{
				shouldShowConsole = true;
			}
		}
	}
}
