using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"JKFrame.dll",
		"System.Core.dll",
		"System.dll",
		"Unity.ResourceManager.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// DelegateList<float>
	// JKFrame.EventModule.MultipleParameterEventInfo<object>
	// JKFrame.ResSystem.<>c__DisplayClass21_0<object>
	// JKFrame.SingletonMono<object>
	// System.Action<AOIAddPlayerEvent>
	// System.Action<AOIRemovePlayerEvent>
	// System.Action<AOIUpdatePlayerCoordEvent>
	// System.Action<InitLocalPlayerEvent>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Action<UnityEngine.Vector2Int>
	// System.Action<float>
	// System.Action<object>
	// System.Action<ulong>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2Int>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,object>
	// System.Collections.Generic.Dictionary<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<ulong,object>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<ulong>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet.Enumerator<ulong>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSet<ulong>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.HashSetEqualityComparer<ulong>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2Int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.ICollection<ulong>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerable<ulong>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEnumerator<ulong>
	// System.Collections.Generic.IEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<ulong>
	// System.Collections.Generic.IList<UnityEngine.Vector2Int>
	// System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<ulong,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.List<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<ulong>
	// System.Collections.Generic.Queue.Enumerator<int>
	// System.Collections.Generic.Queue<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2Int>
	// System.Comparison<UnityEngine.Vector2Int>
	// System.Func<UnityEngine.Bounds,byte>
	// System.Func<object,object>
	// System.Func<object>
	// System.Predicate<UnityEngine.Vector2Int>
	// System.Predicate<object>
	// System.Predicate<ulong>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass60_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass61_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>
	// UnityEngine.ResourceManagement.Util.GlobalLinkedListNodeCache<object>
	// UnityEngine.ResourceManagement.Util.LinkedListNodeCache<object>
	// }}

	public void RefMethods()
	{
		// System.Void JKFrame.EventModule.AddEventListener<object>(string,object)
		// System.Void JKFrame.EventModule.AddMultipleParameterEventInfo<object>(string,object)
		// System.Void JKFrame.EventModule.EventTrigger<AOIAddPlayerEvent>(string,AOIAddPlayerEvent)
		// System.Void JKFrame.EventModule.EventTrigger<AOIRemovePlayerEvent>(string,AOIRemovePlayerEvent)
		// System.Void JKFrame.EventModule.EventTrigger<AOIUpdatePlayerCoordEvent>(string,AOIUpdatePlayerCoordEvent)
		// System.Void JKFrame.EventModule.EventTrigger<InitLocalPlayerEvent>(string,InitLocalPlayerEvent)
		// System.Void JKFrame.EventSystem.AddEventListener<AOIAddPlayerEvent>(string,System.Action<AOIAddPlayerEvent>)
		// System.Void JKFrame.EventSystem.AddEventListener<AOIRemovePlayerEvent>(string,System.Action<AOIRemovePlayerEvent>)
		// System.Void JKFrame.EventSystem.AddEventListener<AOIUpdatePlayerCoordEvent>(string,System.Action<AOIUpdatePlayerCoordEvent>)
		// System.Void JKFrame.EventSystem.AddEventListener<InitLocalPlayerEvent>(string,System.Action<InitLocalPlayerEvent>)
		// System.Void JKFrame.EventSystem.AddTypeEventListener<AOIAddPlayerEvent>(System.Action<AOIAddPlayerEvent>)
		// System.Void JKFrame.EventSystem.AddTypeEventListener<AOIRemovePlayerEvent>(System.Action<AOIRemovePlayerEvent>)
		// System.Void JKFrame.EventSystem.AddTypeEventListener<AOIUpdatePlayerCoordEvent>(System.Action<AOIUpdatePlayerCoordEvent>)
		// System.Void JKFrame.EventSystem.AddTypeEventListener<InitLocalPlayerEvent>(System.Action<InitLocalPlayerEvent>)
		// System.Void JKFrame.EventSystem.EventTrigger<AOIAddPlayerEvent>(string,AOIAddPlayerEvent)
		// System.Void JKFrame.EventSystem.EventTrigger<AOIRemovePlayerEvent>(string,AOIRemovePlayerEvent)
		// System.Void JKFrame.EventSystem.EventTrigger<AOIUpdatePlayerCoordEvent>(string,AOIUpdatePlayerCoordEvent)
		// System.Void JKFrame.EventSystem.EventTrigger<InitLocalPlayerEvent>(string,InitLocalPlayerEvent)
		// System.Void JKFrame.EventSystem.TypeEventTrigger<AOIAddPlayerEvent>(AOIAddPlayerEvent)
		// System.Void JKFrame.EventSystem.TypeEventTrigger<AOIRemovePlayerEvent>(AOIRemovePlayerEvent)
		// System.Void JKFrame.EventSystem.TypeEventTrigger<AOIUpdatePlayerCoordEvent>(AOIUpdatePlayerCoordEvent)
		// System.Void JKFrame.EventSystem.TypeEventTrigger<InitLocalPlayerEvent>(InitLocalPlayerEvent)
		// object JKFrame.ResSystem.InstantiateGameObject<object>(string,UnityEngine.Transform,string,bool)
		// System.Void JKFrame.ResSystem.InstantiateGameObjectAsync<object>(string,System.Action<object>,UnityEngine.Transform,string,bool)
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.Empty<object>()
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
	}
}