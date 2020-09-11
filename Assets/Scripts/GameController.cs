using System.Collections;
//using System.Diagnostics;
using System.IO;
//using System.Numerics;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //Main configuration struct
    private Path _path;

    //Player GameObject
    public GameObject _player;

    //String for TO/FROM JSON conversions
    private string path;
    
    //Can start movement
    private bool canStart = false;

    //Can start movement to next point
    private bool coroutineAllowed = true;

    //Whole path distance
    private float distance;

    //Speed of Player to get path in time
    private float speed;

    //Points of curved angle: A/D - start/end, B/C - controls  
    private Vector3 A, B, C, D;

    //Array of new (curved) path points
    private Vector3[] newPath;

    //Resolution of angle curve (lenth of each line in curve)
    private float resolution = 0.1f;

    //Iterations of curved angle loop
    private int loops;

    //Point to move to in moving coroutine 
    private int currentPoint = 1;

    void Start()
    {
        //Try to get configuration file to JSON string
        //If cant get it, using local backup
        if (File.Exists(Application.dataPath + "/path.json"))
            path = File.ReadAllText(Application.dataPath + "/path.json");
        else
            path = File.ReadAllText(Application.dataPath + "/backUpPath.json");

        //Parse JSON string to Path struct
        _path = JsonUtility.FromJson<Path>(path);

        //Setup needed variables
        loops = Mathf.FloorToInt(1f / resolution);
        int counts = loops * (_path.path.Length - 2) + 2;
        newPath = new Vector3[counts];
        
        //Fill new array and curve angles
        MakeBezier();

        //Get distance of path and calculate Player speed
        distance = getDistance();
        speed = distance / _path.timeToPath;

        //Visualize path in scene
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.startWidth = .2f;
        lineRenderer.endWidth = .2f;
        lineRenderer.positionCount = newPath.Length;

        for (int i = 0; i < newPath.Length; i++)
            lineRenderer.SetPosition(i, newPath[i]);

        //Initial Player rotation to first point
        _player.transform.rotation = Quaternion.LookRotation((newPath[1] - _player.transform.position).normalized);
    }

    void Update()
    {
        //Start moving with key pressed
        if (Input.anyKeyDown)
            canStart = true;

        //Move Player to current point
        if (canStart && coroutineAllowed)
            StartCoroutine(MovePlayer(currentPoint));
    }

    //Player movement logic
    private IEnumerator MovePlayer(int pointToGo)
    {
        //Cant move to next point while current point unreached
        coroutineAllowed = false;

        //Check distance from Player to current point with small precision
        while (Vector3.Distance(_player.transform.position, newPath[pointToGo]) > 0.001f)
        {
            //Rotate Player to facing target point
            _player.transform.rotation = Quaternion.LookRotation((newPath[pointToGo] - _player.transform.position).normalized);

            //Move Player to current point with needed speed
            _player.transform.position = Vector3.MoveTowards(_player.transform.position, newPath[pointToGo], Time.deltaTime * speed);

            //Make movement more smoothly
            yield return new WaitForEndOfFrame();
        }

        //Next point of path array
        currentPoint += 1;

        //If path complete and Player must start it again
        if (currentPoint == newPath.Length && _path.loop)
        {
            currentPoint = 0;
            coroutineAllowed = true;
        }
        else if (currentPoint == newPath.Length && !_path.loop)
        {
            //If path complete and Player must stop at last point
            currentPoint = 0;
            coroutineAllowed = false;
        } else
        {
            //If path incomplete
            coroutineAllowed = true;
        }   
    }


    public void MakeBezier()
    {
        //First and last point in cuved path are same point from initail path
        newPath[0] = _path.path[0];
        newPath[newPath.Length-1] = _path.path[_path.path.Length-1];

        //New path array index
        int _i = 1;

        //Looping old path array (except first/last points)
        for (int i = 1; i < _path.path.Length - 1; i++)
        {
            //Get curve points
            A = FindLineCircleIntersections(_path.path[i], 2f, _path.path[i], _path.path[i-1]);
            B = FindLineCircleIntersections(_path.path[i], 1f, _path.path[i], _path.path[i-1]);
            C = FindLineCircleIntersections(_path.path[i], 1f, _path.path[i], _path.path[i + 1]);
            D = FindLineCircleIntersections(_path.path[i], 2f, _path.path[i], _path.path[i + 1]);

            //Looping current curve with given resolution
            for (int j = 1; j <= loops; j++)
            {
                //Current position (point) of curve
                float t = j * resolution;

                //Find coordinates between the control points with
                Vector3 newPos = DeCasteljausAlgorithm(t);

                //Add current curve point to new path array
                newPath[_i] = newPos;

                //Next array index
                _i++;
            }
        }
    }

    //Calculate distance of path by simple sum of each line (point-point) lenth
    private float getDistance()
    {
        for (int i = 0; i < newPath.Length - 1; i++)
            distance += Vector3.Distance(newPath[i], newPath[i + 1]);
 
        return distance;
    }

    //Find the points between path lines and circles intersection
    //Used for finding start/end and control point of future curve
    private Vector3 FindLineCircleIntersections(
        Vector3 cirCenter, float radius,
        Vector3 point1, Vector3 point2)
    {
        float dx, dz, A, B, C, det, t;

        //Coords of intersection point (return this)
        Vector3 intersection;

        //Solving ((-B + sqrt(B^2 - 4AC)) / 2A) quadratic formula
        dx = point2.x - point1.x;
        dz = point2.z - point1.z;

        A = dx * dx + dz * dz;
        B = 2 * (dx * (point1.x - cirCenter.x) + dz * (point1.z - cirCenter.z));
        C = (point1.x - cirCenter.x) * (point1.x - cirCenter.x) +
            (point1.z - cirCenter.z) * (point1.z - cirCenter.z) -
            radius * radius;

        det = B * B - 4 * A * C;

        t = (float)((-B + Mathf.Sqrt(det)) / (2 * A));

        intersection = new Vector3(point1.x + t * dx, 0, point1.z + t * dz);

        return intersection;
    }

    //The De Casteljau's Algorithm
    //Used to get point of curve by given start/end and control points (A/D B/C)
    private Vector3 DeCasteljausAlgorithm(float t)
    {
        //To make it faster
        float oneMinusT = 1f - t;

        //Layer 1
        Vector3 Q = oneMinusT * A + t * B;
        Vector3 R = oneMinusT * B + t * C;
        Vector3 S = oneMinusT * C + t * D;

        //Layer 2
        Vector3 P = oneMinusT * Q + t * R;
        Vector3 T = oneMinusT * R + t * S;

        //Final interpolated position
        Vector3 U = oneMinusT * P + t * T;

        return U;
    }
}
