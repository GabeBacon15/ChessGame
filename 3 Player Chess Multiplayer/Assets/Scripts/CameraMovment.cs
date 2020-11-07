using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovment : MonoBehaviour
{
    public bool move;
    public float speed;
    public float scalex, scaley;
    private float height;
    private float angleDown;
    public Camera cam;
    public Material hoverMat, killMat;
    public Material[] possibleMat;
    GameObject hoverMesh;
    List<GameObject> possibleMesh;
    public Vector3 point, mouseCoord, mouseClosest;
    public LayerMask clickMask;
    // Start is called before the first frame update
    void Start()
    {
        cam = this.GetComponent<Camera>();
        //move = true;
        height = transform.position.y;
        angleDown = transform.eulerAngles.x;
        point = new Vector3();

        mouseCoord = new Vector3();
        possibleMesh = new List<GameObject>();
        hoverMesh = new GameObject("blank");
        hoverMesh.AddComponent<MeshRenderer>().material = hoverMat;
        hoverMesh.AddComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            float angle = Mathf.Deg2Rad * (Time.time * speed-90);
            transform.position = new Vector3(Mathf.Cos(angle) * scalex, transform.position.y, Mathf.Sin(angle) * scaley);
            transform.rotation = Quaternion.Euler(angleDown, -Mathf.Rad2Deg * angle - 90, 0);
        }
        else
        {
            updatePlayerPoint();
        }
    }


    private void updatePlayerPoint()
    {
        Ray clickRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(clickRay, out hit, 100f, clickMask))
        {
            point = hit.point;
        }
        mouseClosest = Piece.getWorld2Board(getClosest(point));
        if (mouseCoord != mouseClosest)
        {
            mouseCoord = mouseClosest;
            hoverMesh.GetComponent<MeshFilter>().mesh = createMesh(mouseCoord, .2f, .2f);
            hoverMesh.name = "(" + mouseCoord.x + ", " + mouseCoord.y + ", " + mouseCoord.z + ")";
        }
    }

    private Mesh createMesh(Vector3 coord, float height, float peak)
    {
        float triHeight = 0.125f;
        Vector3[] vertecies = new Vector3[14];
        Vector2[] uv;
        int[] triangles;

        Vector3 heightV = new Vector3(0, height, 0);
        Vector3 peakV = new Vector3(0, peak, 0);

        vertecies[0] = Piece.getCoordInWorldSpace(new Vector3(coord.x - .5f, coord.y - .5f, coord.z)) + heightV;
        vertecies[1] = Piece.getCoordInWorldSpace(new Vector3(coord.x + .5f, coord.y - .5f, coord.z)) + heightV;
        vertecies[2] = Piece.getCoordInWorldSpace(new Vector3(coord.x + .5f, coord.y + .5f, coord.z)) + heightV;
        vertecies[3] = Piece.getCoordInWorldSpace(new Vector3(coord.x - .5f, coord.y + .5f, coord.z)) + heightV;
        vertecies[4] = vertecies[0];
        for (int i = 0; i < 5; i++)
            vertecies[i + 5] = vertecies[i] - heightV;
        for (int i = 10; i < 14; i++)
            vertecies[i] = Piece.getCoordInWorldSpace(new Vector3(coord.x, coord.y, coord.z)) + heightV + peakV;

        triangles = new int[]
        {
            //Front
            0, 1, 5,
            1, 6, 5,
            //Right
            1, 2, 6,
            2, 7, 6,
            //Back
            2, 3, 7,
            3, 8, 7,
            //Left
            3, 4, 8,
            4, 9, 8,
            //Top
            10, 1, 0,
            11, 2, 1,
            12, 3, 2,
            13, 4, 3
        };

        uv = new Vector2[]
        {
            new Vector2(0.00f, 1 - triHeight),
            new Vector2(0.25f, 1 - triHeight),
            new Vector2(0.50f, 1 - triHeight),
            new Vector2(0.75f, 1 - triHeight),
            new Vector2(1.00f, 1 - triHeight),

            new Vector2(0.00f, 1f - (height/3) - triHeight),
            new Vector2(0.25f, 1f - (height/3) - triHeight),
            new Vector2(0.50f, 1f - (height/3) - triHeight),
            new Vector2(0.75f, 1f - (height/3) - triHeight),
            new Vector2(1.00f, 1f - (height/3) - triHeight),

            new Vector2(0.125f, 1f),
            new Vector2(0.375f, 1f),
            new Vector2(0.625f, 1f),
            new Vector2(0.875f, 1f)
        };

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertecies;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.Optimize();
        mesh.RecalculateNormals();

        return mesh;
    }
    public Vector3 getClosest(Vector3 position)
    {
        Vector3 shortest = Piece.getBoard2World(new Vector3(0, 0, 0));
        float length = Mathf.Sqrt(Mathf.Pow(position.x - Piece.getBoard2World(new Vector3(0, 0, 0)).x, 2) + Mathf.Pow(position.z - Piece.getBoard2World(new Vector3(0, 0, 0)).z, 2));

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int n = 0; n < 8; n++)
                {
                    float current = Mathf.Sqrt(Mathf.Pow(position.x - Piece.getBoard2World(new Vector3(n, j, i)).x, 2) + Mathf.Pow(position.z - Piece.getBoard2World(new Vector3(n, j, i)).z, 2));
                    if (current < length)
                    {
                        shortest = Piece.getBoard2World(new Vector3(n, j, i));
                        length = current;
                    }
                }
            }
        }
        return shortest;
    }
}
