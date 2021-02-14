using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SphereBot :Bot, IPoolableObject
{
    public void OnPoolCreation()
    {
        
    }

    public void OnRelease()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.forward = Vector3.zero;
    }

    public override void LoadFrame(string binarySave)
    {
        byte[] byteArray = Convert.FromBase64String(binarySave);
        MemoryStream mf = new MemoryStream(byteArray);
        BinaryFormatter bf = new BinaryFormatter();
        SphereFrameData data = (SphereFrameData)bf.Deserialize(mf);
        
        id = new Guid(data.id);

        transform.position = VectorArrayConverter.arrayToVector3(data.position);
        transform.rotation = Quaternion.Euler(VectorArrayConverter.arrayToVector3(data.rotation));
    }

    public override void LoadDiffFrame(string binarySave)
    {
        byte[] byteArray = Convert.FromBase64String(binarySave);
        MemoryStream mf = new MemoryStream(byteArray);
        BinaryFormatter bf = new BinaryFormatter();
        SphereDiffFrameData data = (SphereDiffFrameData)bf.Deserialize(mf);
        
        transform.position = VectorArrayConverter.arrayToVector3(data.position);
        transform.rotation = Quaternion.Euler(VectorArrayConverter.arrayToVector3(data.rotation));
    }
}
