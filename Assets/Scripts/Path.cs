using System;
using UnityEngine;

[Serializable]
public struct Path
{    
    public bool loop;
    public int timeToPath;
    public Vector3[] path;

    //Path cunstructor
    public Path(bool _loop, int _timeToPath, Vector3[] _path)
    {
        loop = _loop;
        timeToPath = _timeToPath;
        path = _path;
    }
}
