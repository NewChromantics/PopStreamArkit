using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.XR.iOS;
#endif



[System.Serializable]
public class UnityEvent_Vector3Array : UnityEngine.Events.UnityEvent<Vector3[]> { }


[System.Serializable]
public class UnityEvent_PositionRotation : UnityEngine.Events.UnityEvent<Vector3, Quaternion> { }


public class ArkitMonitor : MonoBehaviour {

	public UnityEvent_String			OnStatusUpdate;
	public UnityEvent_Vector3Array		OnCloudChanged;
	public UnityEvent_PositionRotation	OnCameraTransformUpdate;

	Vector3[] LastCloud;


	void OnEnable()
	{
		UnityARSessionNativeInterface.ARFrameUpdatedEvent += OnUpdate;
		/*
		Session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

		Application.targetFrameRate = 60;
		var config = new ARKitWorldTrackingSessionConfiguration();
		config.planeDetection = planeDetection;
		config.alignment = startAlignment;
		config.getPointCloudData = getPointCloud;
		config.enableLightEstimation = enableLightEstimation;

		if (!config.IsSupported)
		{
			OnStatusUpdate.Invoke("Config not supported");
			throw new System.Exception("Config not supported");
		}

		Session.RunWithConfig(config);
		*/
	}

	void OnDisable()
	{
		UnityARSessionNativeInterface.ARFrameUpdatedEvent += OnUpdate;
	}


	//	returns if changed
	bool UpdatePointCloud(Vector3[] NewCloud)
	{
		var NewLength = (NewCloud != null) ? NewCloud.Length : 0;
		var LastLength = (LastCloud != null) ? LastCloud.Length : 0;
		if ( NewLength != LastLength )
		{
			LastCloud = NewCloud;
			return true;
		}

		//	probably null
		if ( NewLength == 0 )
		{
			LastCloud = NewCloud;
			return true;
		}

		//	check diff
		var MinChange = float.Epsilon;
		var Same = true;
		for (var i = 0; i < LastCloud.Length;	i++ )
		{
			var LastPos = LastCloud[i];
			var NewPos = NewCloud[i];
			var Change = Vector3.Distance(NewPos, LastPos);
			if (Change <= MinChange)
				continue;
			Same = false;
			break;
		}

		LastCloud = NewCloud;
		return Same;
	}

	void OnUpdate(UnityARCamera cam)
	{
		var NewCloudData = cam.pointCloudData;
		var CloudChanged = UpdatePointCloud(NewCloudData);
		if ( CloudChanged )
		{
			OnCloudChanged.Invoke(LastCloud);
		}

		//	report state
		var State = "";
		var ReasonString = cam.trackingReason.ToString().Substring("ARTrackingStateReason".Length);
		switch ( cam.trackingState )
		{
			case ARTrackingState.ARTrackingStateNormal:
				State = "Tracking";
				break;

			case ARTrackingState.ARTrackingStateNotAvailable:
				State = "Not availible (" + ReasonString + ")";
				break;

			case ARTrackingState.ARTrackingStateLimited:
				State = "Limited tracking (" + ReasonString + ")";
				break;
		}
		OnStatusUpdate.Invoke(State);

		if (cam.trackingState != ARTrackingState.ARTrackingStateNormal)
			return;

		//	get new pose
		var WorldMatrixArkit = cam.worldTransform;
		var WorldMatrix = new Matrix4x4();
		WorldMatrix.SetColumn(0, WorldMatrixArkit.column0);
		WorldMatrix.SetColumn(1, WorldMatrixArkit.column1);
		WorldMatrix.SetColumn(2, WorldMatrixArkit.column2);
		WorldMatrix.SetColumn(3, WorldMatrixArkit.column3);

		var WorldPosition = UnityARMatrixOps.GetPosition(WorldMatrix);
		var WorldRotation = UnityARMatrixOps.GetRotation(WorldMatrix);

		OnCameraTransformUpdate.Invoke( WorldPosition, WorldRotation );
	}
}
