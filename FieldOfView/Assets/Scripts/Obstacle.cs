using System;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Transform viewer;

    private MeshFilter filter;

    private Vector3[] vertices;
    private Vector3[] shadowVertices;
    private int meshVerticesCount;

    private readonly Vector3[] convexHull = new Vector3[128];

    private void Start()
    {
        if (!viewer)
        {
            viewer = GameObject.Find("Player").transform;
        }

        filter = GetComponentInChildren<MeshFilter>();
        filter.transform.parent = null;
        filter.transform.localScale = Vector3.one;
        filter.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        filter.mesh = new Mesh();

        Collider2D collider = GetComponent<Collider2D>();
        Mesh mesh = collider.CreateMesh(true, true);
        vertices = mesh.vertices;
        meshVerticesCount = vertices.Length;
    }

    private void Update()
    {
        ShadowCast();
    }

    private void ShadowCast()
    {
        // 오브젝트의 메시 버텍스를 시점과 50배 먼 위치에도 똑같이 복사한 뒤
        // 점들에서 볼록 다각형을 구해 동적으로 그림자 생성
        // 중간 어느 한 점이 들어간 오목 다각형은 제대로 구할 수 없음

        shadowVertices = new Vector3[meshVerticesCount * 2];
        for (int i = 0; i < meshVerticesCount; i++)
        {
            shadowVertices[i] = vertices[i];
            shadowVertices[i + meshVerticesCount] = (vertices[i] - viewer.position) * 50f;
        }

        int n;
        try
        {
            n = Geometry2D.ConvexHull(shadowVertices, convexHull);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            gameObject.SetActive(false);
            return;
        }

        // 구한 다각형에서 n - 2개의 삼각형 구역 나누기
        // 다각형의 점들이 어떤 방향으로든 반드시 정렬되어 있어야 함
        // triangles[0] = 0 \
        // triangles[1] = 1  ) --- 삼각형 하나의 점 인덱스
        // triangles[2] = 2 /
        // triangles[3] = 0
        // triangles[4] = 2
        // triangles[5] = 3 ...
        int trianglesCnt = n - 2;
        int[] triangles = new int[trianglesCnt * 3];
        for (int i = 0; i < trianglesCnt; i++)
        {
            int index = i * 3;
            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2;
        }
        
        // 현재 점의 수가 이전 점의 수보다 작으면
        // 삼각형이 참조하는 인덱스의 범위를 벗어나버리는 에러가 있음
        if (n < filter.mesh.vertexCount)
        {
            filter.mesh.SetTriangles(triangles, 0);
            filter.mesh.SetVertices(convexHull, 0, n);
        }
        else
        {
            filter.mesh.SetVertices(convexHull, 0, n);
            filter.mesh.SetTriangles(triangles, 0);
        }
        //치명적이지는 않으나 디버그에 빨간 불 켜지는 거 싫으면 하기
    }
}