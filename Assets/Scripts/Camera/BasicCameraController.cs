using System;
using AdvancedUtilities.Cameras.Components;
using AdvancedUtilities.Cameras;
using UnityEngine;

namespace Leonhartz
{
	/// <summary>
	/// A basic camera controller.
	/// </summary>
	[Serializable]
	public class BasicCameraController : CameraController
	{
		[Header("Settings")]
		/// <summary>
		/// The camera focus tag.
		/// </summary>
		[Tooltip("The tag of the object that the camera should look at.")]
		public string CameraFocusTag = "CameraFocus";
		/// <summary>
		/// The distance that the camera wants to position itself at from the target.
		/// </summary>

		[Tooltip("The distance that the camera wants to position itself at from the target.")]
		public float DesiredDistance = 20f;

		/// <summary>
		/// The minimum that zooming will let you zoom in to.
		/// </summary>
		[Tooltip("The minimum that zooming will let you zoom in to.")]
		public float MinZoomDistance = 0f;

		/// <summary>
		/// The maximum that zooming will let you zoom out to.
		/// </summary>
		[Tooltip("The maximum that zooming will let you zoom out to.")]
		public float MaxZoomDistance = 50f;

		/// <summary>
		/// When the CameraController starts, horizontal rotation will be set to this value.
		/// </summary>
		//[Tooltip("When the CameraController starts, horizontal rotation will be set to this value.")]
		//public float InitialHorizontalRotation = 0f;

		/// <summary>
		/// When the CameraController starts, vertical rotation will be set to this value.
		/// </summary>
		[Tooltip("When the CameraController starts, vertical rotation will be set to this value.")]
		public float InitialVerticalRotation = 35f;

		#region Components
		/// <summary>
		/// TargetComponent
		/// </summary>
		[Header("Components")]
		public TargetComponent Target;
		/// <summary>
		/// RotationComponent
		/// </summary>
		public RotationComponent Rotation;
		/// <summary>
		/// ZoomComponent
		/// </summary>
		public ZoomComponent Zoom;
		/// <summary>
		/// ViewCollisionComponent
		/// </summary>
		public ViewCollisionComponent ViewCollision;
		/// <summary>
		/// InputComponent
		/// </summary>
		public InputComponent Input;
		/// <summary>
		/// EasyUnityInputComponent
		/// </summary>
		public EasyUnityInputComponent EasyUnityInput;
		/// <summary>
		/// CursorComponent
		/// </summary>
		public CursorComponent Cursor;
		/// <summary>
		/// HeadbobComponent
		/// </summary>
		public HeadbobComponent Headbob;
		/// <summary>
		/// ScreenShakeComponent
		/// </summary>
		public ScreenShakeComponent ScreenShake;

		#endregion

		/// <summary>
		/// The previous distance the camera was at during the last update.
		/// </summary>
		private float _previousDistance;

		public static BasicCameraController instance;
		bool ready = false;

		protected override void AddCameraComponents()
		{
			AddCameraComponent(Rotation);
			AddCameraComponent(Zoom);
			AddCameraComponent(Target);
			AddCameraComponent(ViewCollision);
			AddCameraComponent(Input);
			AddCameraComponent(EasyUnityInput);
			AddCameraComponent(Cursor);
			AddCameraComponent(Headbob);
			AddCameraComponent(ScreenShake);
		}

		void Start()
		{
			instance = this;
		}

		void Init(){
			GameObject go = GameObject.FindGameObjectWithTag (CameraFocusTag);
			if (go != null) {
				Target.Target = go.transform;

				// Set initial rotation and distance
				Rotation.Rotate(go.transform.rotation.eulerAngles.y, InitialVerticalRotation);
				_previousDistance = DesiredDistance;

				ready = true;
	
				UpdateCamera();
			}
		}

		void LateUpdate()
		{
			if (!ready) {
				Init ();
				return;
			}
			if (!Target.Target) {
				ready = false;
				Init ();
				return;
			}

			UpdateCamera();

			CameraTransform.ApplyTo(Camera); // Apply the virtual transform to the actual transform
		}

		public override void UpdateCamera()
		{
			// Get Input
			EasyUnityInput.AppendInput();
			InputValues input = Input.ProcessedInput;
			Input.ClearInput();

			// Handle Rotating
			if (input.Horizontal.HasValue)
			{
				Rotation.RotateHorizontally(input.Horizontal.Value);
			}
			if (input.Vertical.HasValue)
			{
				Rotation.RotateVertically(input.Vertical.Value);
			}

			Rotation.CheckRotationDegreesEvents();

			// Apply target offset modifications
			Vector3 headbobOffset = Headbob.GetHeadbobModifier(_previousDistance);
			Target.AddWorldSpaceOffset(headbobOffset);
			Vector3 screenShakeOffset = ScreenShake.GetShaking();
			Target.AddWorldSpaceOffset(screenShakeOffset);

			// Handle Cursor
			Cursor.SetCursorLock();

			// Hanlde Zooming
			if (input.ZoomIn.HasValue)
			{
				DesiredDistance = Mathf.Max(DesiredDistance + input.ZoomIn.Value, 0);
				DesiredDistance = Mathf.Max(DesiredDistance, MinZoomDistance);
			}
			if (input.ZoomOut.HasValue)
			{
				DesiredDistance = Mathf.Min(DesiredDistance + input.ZoomOut.Value, MaxZoomDistance);
			}

			// Get target
			Vector3 target = Target.GetTarget();
			float actual = _previousDistance;

			// Set Camera Position
			float desired = DesiredDistance; // Where we want the camera to be
			float calculated = ViewCollision.CalculateMaximumDistanceFromTarget(target, Mathf.Max(desired, actual)); // The maximum distance we calculated we can be based off collision and preference
			float zoom = Zoom.CalculateDistanceFromTarget(actual, calculated, desired); // Where we want to be for the sake of zooming

			CameraTransform.Position = target - CameraTransform.Forward * zoom; // Set the position of the transform

			_previousDistance = Target.GetDistanceFromTarget();
			Target.ClearAdditionalOffsets();
		}
	}
}
