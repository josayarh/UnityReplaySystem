# Introduction 

The unity project included with this readme is a framework for a live save and replay framework.
Included in the project are the base scripts necessary for the framework and 2 exemples of implementation, one for "gameplay" purposes and the other for stress testing purposes. 

# How to use the framework 

The framework is divided into 3 main components, the save component saves the data that you want to record, the bot component is an object controller 
that replays the data and the gameStateObjectManager saves the combined data and handles the recreation of the objects on restart. 

## Save component 
The save component as illustrated in SphereSave must inherit from SavableObject and implement MakeFrame() and MakeDiffFrame(). Those methods are used to create the save data
for every frame the object is active. The data must be defined in a serializable structure as shown in SphereFrameData.cs, and then turned into a binary string, see the SaveFrameData 
in SphereSave.cs for an exemple. 

MakeFrame is only used on the first frame to save static variables whose value will not change over the course of the game, MakeDiffFrame should be called in FixedUpdate to save the dynamic data of the object.
In the project for exemple we save the object's id only in MakeFrame whereas we save the object's position in both functions. 

## Bot Component 
The bot component is in charge of replaying the data, the replay itself is already taken care of in the base class so the only thing to implement is the deseriazation process via the LoadFrame(string binarySave) and 
LoadDiffFrame(string binarySave) methods. Those methods take a string corresponding to a binary save made in one of the save components and load it when necessary. 
A simple exemple of this process is provided in the SphereBot script. 

## GameObjectStateManager 
The GameObjectStateManager saves and reloads objects when necessary. When a save component is destroyed it's data must be saved to the GameObjectStateManager using the GameObjectStateManager.Instance.addDynamicObject
method. In fixed update the reloadObjects() will be called, this method will look among the saved data to check at which frame the object was saved, if it corresponds to the current frame th system will try to restore it. 
In order for custom objects to be restored the user needs to add his objects to the reloadObjects method specifically in this section : 

if (parentIds.TryGetValue(id, out tryGetParentGuid))
{
	if (!doesParentExist(id))
		break;
}
else
{
	if (gameObjectTuple.Item1 == typeof(SphereSave))
	{
		go = Pool.Instance.get(PoolableTypes.SphereBot);
		if (go != null)
		{
			SphereBot bulletBotController =
				go.GetComponent<SphereBot>();
			if (gameObjectTuple.Item2 == null)
			{
				Debug.Log("No frame list associated with this object");
			}
			
			bulletBotController.FrameSteps = gameObjectTuple.Item2;
		}
		else
		{
			Debug.Log("No sphere bot left in pool");
		}
	}
}

Starting from if (gameObjectTuple.Item1 == typeof(SphereSave)) is the code used to restore saved objects, if the object if of the Save type then it creates a corresponding bot type. Unfortunately this process is not 
automated and has to be implemented for every new object that needs to be saved. 