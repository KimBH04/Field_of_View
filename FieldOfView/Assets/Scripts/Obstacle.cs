using System;
using System.Collections.Generic;
using System.Linq;
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
        shadowVertices = new Vector3[meshVerticesCount * 2];
        for (int i = 0; i < meshVerticesCount; i++)
        {
            shadowVertices[i] = vertices[i];
            shadowVertices[i + meshVerticesCount] = (vertices[i] - viewer.position) * 50f;
        }

        int n = ConvexHull(shadowVertices, convexHull);

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

    /// <summary>
    /// 가장 큰 볼록 다각형 구하기<br/>
    /// x, y값만 취하고 z값은 버림<br/>
    /// </summary>
    /// <param name="vertices"> 모든 점들 </param>
    /// <param name="result"> 볼록 다각형을 이루는 점들의 위치 </param>
    /// <returns> 볼록 다각형을 이루는 점의 개수 </returns>
    private static int ConvexHull(Vector3[] vertices, Vector3[] result)
    {
        // 그라함 스캔
        // 특) 버그가 좀 있다
        Vector3 min = vertices[0];
        foreach (Vector3 v in vertices[1..])
        {
            if (min.y > v.y)
            {
                min = v;
            }
        }

        // 가장 밑에 있는 점 기준으로 오른쪽으로 회전한 순서, 먼 거리 순서로 정렬
        var sorted = vertices
                    .Where(v => v != min)
                    .OrderBy(v => (CounterClockWise(min + Vector3.down, min, v), -Vector3.SqrMagnitude(min - v)));

        // 다음 점이 이전 두 점을 지나는 선보다 오른쪽에 있으면 스택에서 점 하나 제거
        List<Vector3> stack = new() { min };
        foreach (Vector3 v in sorted)
        {
            while (stack.Count >= 2 && CounterClockWise(stack[^2], stack[^1], v) < 0f)
            {
                stack.RemoveAt(stack.Count - 1);
            }
            stack.Add(v);
        }

        // 시작점과 비교
        while (stack.Count >= 2 && CounterClockWise(stack[^2], stack[^1], min) < 0f)
        {
            stack.RemoveAt(stack.Count - 1);
        }

        int len = stack.Count;
        if (result.Length < len)
        {
            throw new ArgumentOutOfRangeException("Result array is too small.");
        }

        for (int i = 0; i < len; i++)
        {
            result[i] = stack[i];
        }

        return len;
    }

    /// <summary>
    /// 세 점 a-b-c를 잇는 꺾은선의 꺾인 방향 구하기
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns>
    /// 왼쪽 = 양수<br/>
    /// 오른쪽 = 음수<br/>
    /// 직선 = 0
    /// </returns>
    private static float CounterClockWise(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v1 = Vector3.Normalize(b - a);
        Vector3 v2 = Vector3.Normalize(c - b);

        return v1.x * v2.y - v2.x * v1.y;
    }
}