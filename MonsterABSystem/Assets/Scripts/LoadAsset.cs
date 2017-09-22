using UnityEngine;
using System.Collections;
using Monster.BaseSystem;
using Monster.BaseSystem.CoroutineTask;

public class LoadAsset : MonoBehaviour
{

    private AssetLoadTask _task;
	// Use this for initialization
	void Start () {
        _task = new AssetLoadTask();
        StartCoroutine(_task.Run());
	}

    private bool hasAdd = false;
	// Update is called once per frame
	void Update () {

        if (_task.IsCompleted && !hasAdd)
        {
            hasAdd = true;
	        ResourcesFacade.Instance.Init(null);
	        var panel = ResourcesFacade.Instance.LoadPrefab("Plane");
	        panel.transform.parent = transform;
            panel.transform.localPosition = Vector3.zero;
	        panel.transform.localEulerAngles = Vector3.zero;
	    }
	}
}
