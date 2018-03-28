using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ArkitCameraTransform : BasePacket
{
	public Vector3		Position;
	public Quaternion	Rotation;
	
	public ArkitCameraTransform(Transform transform,string StreamName)
	{
		//	this shows that PopRelayClient and PopRelayDecoder probbaly need Encoding() funcs
		//	encoding is json, but OUR decoder figures that out
		//	Data is also unused, we just re-decode the original json
		this.Timecode = PopX.Time.GetTodayUtcTimeMs();
		this.Encoding = this.GetType().ToString();
		this.Data = null;
		this.Stream = StreamName;
	}

}

public class ArkitStreamer : MonoBehaviour {

	PopRelayClient				Client { get { return GameObject.FindObjectOfType<PopRelayClient>(); }}
	public RepeatActionTimer	Timer;
	public Transform			ArkitCameraTransform;
	public string				StreamName = "ArkitCameraTransform";

	void OnEnable()
	{
		if (ArkitCameraTransform == null)
			ArkitCameraTransform = Camera.main.transform;
		if (string.IsNullOrEmpty(StreamName))
			StreamName = this.name;
	}

	void SendCameraPosition()
	{
		var Packet = new ArkitCameraTransform(ArkitCameraTransform, StreamName);
		if ( Client.IsConnected)
			Client.SendJson( Packet );
	}


	void Update()
	{
		Timer.Update(SendCameraPosition);
	}
}
