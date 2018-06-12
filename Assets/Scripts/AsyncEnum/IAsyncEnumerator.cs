using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IAsyncEnumerator<T> : IDisposable
{

    T Current { get; }

    Task<bool> MoveNext(CancellationToken cancelToken);
}
