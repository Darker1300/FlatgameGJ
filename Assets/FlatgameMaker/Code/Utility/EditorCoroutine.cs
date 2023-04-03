using System;
using System.Collections;

/// <summary>
/// Makes it possible to run coroutines while inside an editor script
/// using the EditorApplication.update callback.
/// </summary>
public class EditorCoroutine
{
	private readonly IEnumerator routine;
	public bool running = false;
	
	private EditorCoroutine( IEnumerator routine )
	{
		this.routine = routine;
	}
	
	/// <summary>
	/// Start an editor coroutine.
	/// </summary>
	/// <param name="routine"></param>
	/// <returns></returns>
	public static EditorCoroutine Start( IEnumerator routine )
	{
		EditorCoroutine coroutine = new EditorCoroutine( routine );
		coroutine.Start();
		coroutine.Update();
		return coroutine;
	}
	
	/// <summary>
	/// Stop this editor coroutine.
	/// </summary>
	public void Stop()
	{
		running = false;
#if UNITY_EDITOR
		UnityEditor.EditorApplication.update -= Update;
#endif
	}
	
	private void Start()
	{
		running = true;
#if UNITY_EDITOR
		UnityEditor.EditorApplication.update += Update;
#endif
	}
	
	private void Update()
	{
		if ( !routine.MoveNext() )
		{
			Stop();
		}
	}
}