using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SphereSave : SavableObject ,IPoolableObject
{
    // Start is called before the first frame update
    void Start()
    {
    }

    private void FixedUpdate()
    {
        SaveDiffFrame();
    }
    
    private void SaveFrame()
    {
        if(id != Guid.Empty)
            frameSaveList.Add(MakeFrame());
    }
    
    private void SaveDiffFrame()
    {
        string diffFrame;
        diffFrame = MakeDiffFrame();

        frameSaveList.Add(diffFrame);
    }

    public override string MakeFrame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        
        SphereFrameData data = new SphereFrameData();
        
        data.id = new Byte[id.ToByteArray().Length];
        id.ToByteArray().CopyTo(data.id,0);
        
        data.position = VectorArrayConverter.vector3ToArray(transform.position);
        data.rotation = VectorArrayConverter.vector3ToArray(transform.rotation.eulerAngles);
        
        bf.Serialize(ms,data);

        return Convert.ToBase64String(ms.ToArray());
    }

    public override string MakeDiffFrame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        
        SphereDiffFrameData data = new SphereDiffFrameData();
        
        data.position = VectorArrayConverter.vector3ToArray(transform.position);
        data.rotation = VectorArrayConverter.vector3ToArray(transform.rotation.eulerAngles);
        
        bf.Serialize(ms,data);

        return Convert.ToBase64String(ms.ToArray());
    }

    public void OnPoolCreation()
    {
        id = Guid.NewGuid();

        SaveFrame();
    }

    public void OnRelease()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.forward = Vector3.zero;
        
        if (id != Guid.Empty )
        {
            GameObjectStateManager.Instance.addDynamicObject(id, GetType(), frameSaveList, 0);
            frameSaveList = new List<string>();
        }
    }
}
