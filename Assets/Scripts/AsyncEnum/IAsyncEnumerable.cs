using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAsyncEnumerable<T>
{
	IAsyncEnumerator<T> GetEnumerator();
}
