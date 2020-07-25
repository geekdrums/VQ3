using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SerializableDictionary<TKey, TValue, ListType> : IEnumerable<KeyValuePair<TKey, TValue>>
	where ListType : SerializableKeyValuePair<TKey, TValue>
{
	[SerializeField]
	private List<ListType> list = new List<ListType>();

	private Dictionary<TKey, TValue> dictionary
	{
		get
		{
			if( dict_ == null )
			{
				dict_ = ConvertListToDictionary(list);
			}
			return dict_;
		}
		set
		{
			dict_ = value;
		}
	}
	private Dictionary<TKey, TValue> dict_;
	
	public TValue this[TKey key]
	{
		get
		{
			return dictionary[key];
		}
		set
		{
			dictionary[key] = value;
#if UNITY_EDITOR
			if( !UnityEditor.EditorApplication.isPlaying )
			{
				foreach( SerializableKeyValuePair<TKey, TValue> pair in list )
				{
					if( pair.Key.Equals(key) )
					{
						pair.Value = value;
						break;
					}
				}
			}
#endif
		}
	}
	public bool ContainsKey(TKey key)
	{
		return dictionary.ContainsKey(key);
	}
	public void Remove(TKey key)
	{
		dictionary.Remove(key);
#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying )
		{
			list.RemoveAt(list.FindIndex((pair) => pair.Key.Equals(key)));
		}
#endif
	}
	public void Add(ListType pair)
	{
		dictionary.Add(pair.Key, pair.Value);
#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying )
		{
			list.Add(pair);
		}
#endif
	}
	public TValue At(int index) { return list[index].Value; }


	public IEnumerable<TValue> Values { get { return dictionary.Values; } }
	public IEnumerable<TKey> Keys { get { return dictionary.Keys; } }
	public int Count { get { return dictionary.Count; } }
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}


	public void OnValidate()
	{
		bool needToReflesh = false;
		if( dict_ == null || list.Count != dict_.Count )
		{
			needToReflesh = true;
		}
		else
		{
			foreach( SerializableKeyValuePair<TKey, TValue> pair in list )
			{
				if( dict_.ContainsKey(pair.Key) )
				{
					// キーがある
					if( dict_[pair.Key].Equals(pair.Value) == false )
					{
						// けど値が違う
						// でも同じキーがリスト内に追加されてしまっているだけかもしれない
						if( pair.IsAddedToDict )
						{
							// tableに追加されているはずの要素なら、
							// Valueが変化したということ
							needToReflesh = true;
							break;
						}
						else
						{
							// そうでなければ、まだキーがかぶっている状態で値だけが編集されているので、
							// Dictionary自体は変化しない
						}
					}
				}
				else
				{
					// キーが無いから追加しなきゃ
					needToReflesh = true;
					break;
				}
			}
		}

		if( needToReflesh )
		{
			dict_ = ConvertListToDictionary(list);
		}
	}

	static Dictionary<TKey, TValue> ConvertListToDictionary(List<ListType> list)
	{
		Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
		foreach( SerializableKeyValuePair<TKey, TValue> pair in list )
		{
			if( dict.ContainsKey(pair.Key) )
			{
				pair.IsAddedToDict = false;
				continue;
			}
			dict.Add(pair.Key, pair.Value);
			pair.IsAddedToDict = true;
		}
		return dict;
	}

	public bool KeyEquals(SerializableDictionary<TKey, TValue, ListType> other)
	{
		foreach(var key in this.Keys )
		{
			if( other.ContainsKey(key) == false )
			{
				return false;
			}
		}
		foreach( var key in other.Keys )
		{
			if( this.ContainsKey(key) == false )
			{
				return false;
			}
		}
		return true;
	}

	public void Clone(SerializableDictionary<TKey, TValue, ListType> other)
	{
		list.Clear();
		list.AddRange(other.list);
		dict_ = ConvertListToDictionary(list);
	}

}
	
[System.Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
	public TKey Key;
	public TValue Value;

	public bool IsAddedToDict { get; set; }

	public SerializableKeyValuePair(TKey key, TValue value)
	{
		Key = key;
		Value = value;
	}
	public SerializableKeyValuePair(KeyValuePair<TKey, TValue> pair)
	{
		Key = pair.Key;
		Value = pair.Value;
	}
}

// ジェネリックを隠蔽することで実際にシリアライズ可能なクラスになる
[System.Serializable]
public class StringPair : SerializableKeyValuePair<string, string> { public StringPair(string key, string value) : base(key, value) { } }

[System.Serializable]
public class SerializableStringDictionary : SerializableDictionary<string, string, StringPair> { }

[System.Serializable]
public class StringFloatPair : SerializableKeyValuePair<string, float> { public StringFloatPair(string key, float value) : base(key, value) { } }

[System.Serializable]
public class SerializableStringToFloatDictionary : SerializableDictionary<string, float, StringFloatPair> { }
