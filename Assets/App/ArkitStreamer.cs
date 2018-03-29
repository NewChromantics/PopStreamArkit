using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class ArkitCameraTransform
{
	public Vector3 Position;
	public Quaternion Rotation;

	public ArkitCameraTransform(Transform transform)
	{
		this.Position = transform.localPosition;
		this.Rotation = transform.localRotation;
	}

	public ArkitCameraTransform(Vector3 Position,Quaternion Rotation)
	{
		this.Position = Position;
		this.Rotation = Rotation;
	}
}

[System.Serializable]
public class ArkitPointCloud
{
	public Vector3[] Positions;

	public ArkitPointCloud(Vector3[] Positions)
	{
		this.Positions = Positions;
	}
}


[System.Serializable]
public class ArkitCameraTransformPacket : BasePacket
{
	public ArkitCameraTransform Transform;

	public static string EncodingName { get { return typeof(ArkitCameraTransform).ToString(); } }

	public ArkitCameraTransformPacket(Vector3 Position,Quaternion Rotation, string StreamName)
	{
		//	this shows that PopRelayClient and PopRelayDecoder probbaly need Encoding() funcs
		//	encoding is json, but OUR decoder figures that out
		//	Data is also unused, we just re-decode the original json
		this.Timecode = PopX.Time.GetTodayUtcTimeMs();
		this.Encoding = EncodingName;
		this.Data = null;
		this.Stream = StreamName;
		this.Transform = new ArkitCameraTransform(Position,Rotation);
	}

}


[System.Serializable]
public class ArkitPointCloudPacket : BasePacket
{
	public ArkitPointCloud PointCloud;

	public static string EncodingName { get { return typeof(ArkitPointCloud).ToString(); } }

	public ArkitPointCloudPacket(Vector3[] PointCloud,string StreamName)
	{
		//	this shows that PopRelayClient and PopRelayDecoder probbaly need Encoding() funcs
		//	encoding is json, but OUR decoder figures that out
		//	Data is also unused, we just re-decode the original json
		this.Timecode = PopX.Time.GetTodayUtcTimeMs();
		this.Encoding = EncodingName;
		this.Data = null;
		this.Stream = StreamName;
		this.PointCloud = new ArkitPointCloud(PointCloud);
	}

}

public class ArkitStreamer : MonoBehaviour {

	PopRelayClient				Client { get { return GameObject.FindObjectOfType<PopRelayClient>(); }}

	public ArkitMonitor			Monitor	{ get { return GameObject.FindObjectOfType<ArkitMonitor>(); } }

	public string				PointCloudStreamName = "ArkitPointCloud";
	public string				CameraTransformStreamName = "ArkitCameraTransform";

	void OnEnable()
	{
		Monitor.OnCloudChanged.AddListener(OnCloudChanged);
		Monitor.OnCameraTransformUpdate.AddListener(OnTransformChanged);
	}

	void OnDisable()
	{
		Monitor.OnCloudChanged.RemoveListener(OnCloudChanged);
		Monitor.OnCameraTransformUpdate.RemoveListener(OnTransformChanged);
	}

	void OnCloudChanged(Vector3[] Points)
	{
		var Packet = new ArkitPointCloudPacket(Points, PointCloudStreamName);
		if (Client.IsConnected)
			Client.SendJson(Packet);
	}

	void OnTransformChanged(Vector3 Positon,Quaternion Rotation)
	{
		var Packet = new ArkitCameraTransformPacket(Positon, Rotation, CameraTransformStreamName);
		if (Client.IsConnected)
			Client.SendJson(Packet);
	}


}
