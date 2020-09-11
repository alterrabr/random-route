using System.IO;
using UnityEngine;
using UnityEngine.UI;
//using System.Numerics;

public class PathConstructor : MonoBehaviour
{
    //Main path struct
    private Path _path;

    //New path points count
    private int pointsCount;

    //UI elements
    public Slider pointsSlider;
    public Slider timeSlider;
    public Toggle loopTogle;

    //New path JSON string
    private string path;

    //Generating new path and upload/local backup it
    public void MakeJSON()
    {
        //Get values from UI elements and parse to struct
        pointsCount = (int)pointsSlider.value;
        _path.loop = loopTogle.isOn;
        _path.timeToPath = (int)timeSlider.value;
        _path.path = new Vector3[pointsCount];

        //Make random points
        RandomPoints();

        //Parce struct to JSON string
        path = JsonUtility.ToJson(_path);

        //If no internet connection -> make local backup
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection! Write to local file.");
            File.WriteAllText(Application.dataPath + "/backUpPath.json", path);

        } else
        {
            //Else -> upload to Dropbox app folder
            StartCoroutine(WebRequests.Upload(path));

        }
    }

    //Make random points
    public void RandomPoints()
    {
        //First point always (0, 0, 0)
        _path.path[0] = Vector3.zero;

        //Second and third points (line) cant intersect -> random without conditions
        //
        //TODO: Second and third points can be closer than minimum distance allowed (2f here)
        //So they can intersect after curving. Need to fix this issue
        //
        _path.path[1] = new Vector3(Random.Range(-99, 99), 0, Random.Range(-99, 99));
        _path.path[2] = new Vector3(Random.Range(-99, 99), 0, Random.Range(-99, 99));

        //Randomize positions of the rest points with intersect and minimum distance conditions
        for (int i = 3; i < pointsCount; i++)
        {
            //Iteration goto point if conditions not met 
            again:

            Vector3 tmpPoint = new Vector3(Random.Range(-99, 99), 0, Random.Range(-99, 99));

            //Check for conditions
            for (int j = 0; j < i - 2; j++)
            {
                if (PathIntersection(_path.path[j], _path.path[j + 1], _path.path[i - 1], tmpPoint) || Vector3.Distance(_path.path[j], tmpPoint) < 2f)
                {
                    //If not met make iteration again
                    goto again; 
                } else
                {
                    //If met -> save point to struct
                    _path.path[i] = tmpPoint;
                }
            }
        }
    }

    //Check for lines intersection
    //If 2 input lines (4 start/end points) intersect -> function return true
    public static bool PathIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        var d = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.z - p3.z) - (p3.z - p1.z) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.z - p1.z) - (p3.z - p1.z) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        return true;
    }
}
