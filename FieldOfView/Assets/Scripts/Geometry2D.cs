using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public static class Geometry2D
{
    /// <summary>
    /// 가장 큰 볼록 다각형 구하기<br/>
    /// </summary>
    /// <param name="vertices"> 모든 점들 </param>
    /// <param name="result"> 볼록 다각형을 이루는 점들의 위치 </param>
    /// <returns> 볼록 다각형을 이루는 점의 개수 </returns>
    public static int ConvexHull(Vector2[] vertices, Vector2[] result)
    {
        // 점의 개수가 너무 적음!
        if (vertices.Length < 3)
        {
            throw new ArgumentException("Too few dots.");
        }

        // 모노톤 체인

        // 왼쪽에서 오른쪽으로, 위에서 아래로 정렬
        IEnumerable<Vector2> dots = vertices.OrderBy(v => (v.x, v.y));

        List<Vector2> resultList = new(vertices.Length);

        // 왼쪽에서부터 안으로 꺾이는 점들 탐색
        foreach (Vector2 v in dots)
        {
            while (resultList.Count >= 2 && CounterClockWise(resultList[^2], resultList[^1], v) > 0f)
            {
                resultList.RemoveAt(resultList.Count - 1);
            }
            resultList.Add(v);
        }

        // 오른쪽에서부터 안으로 꺾이는 점들 탐색
        foreach (Vector2 v in dots.Reverse())
        {
            while (resultList.Count >= 2 && CounterClockWise(resultList[^2], resultList[^1], v) > 0f)
            {
                resultList.RemoveAt(resultList.Count - 1);
            }
            resultList.Add(v);
        }

        // 중복 제거
        IEnumerable<Vector2> distinctResult = resultList.Distinct();
        int len = distinctResult.Count();

        // 결과를 담을 배열이 너무 작음!
        if (len > result.Length)
        {
            throw new ArgumentOutOfRangeException("Result array's size is too small.");
        }

        for (int i = 0; i < len; i++)
        {
            result[i] = distinctResult.ElementAt(i);
        }
        return len;
    }

    /// <summary>
    /// 가장 큰 볼록 다각형 구하기<br/>
    /// x, y값만 취하고 z값은 버림
    /// </summary>
    /// <param name="vertices"> 모든 점들 </param>
    /// <param name="result"> 볼록 다각형을 이루는 점들의 위치 </param>
    /// <returns> 볼록 다각형을 이루는 점의 개수 </returns>
    public static int ConvexHull(Vector3[] vertices, Vector3[] result)
    {
        Vector2[] v2result = new Vector2[result.Length];
        int len = ConvexHull(vertices.Select(v => (Vector2)v).ToArray(), v2result);
        for (int i = 0; i < len; i++)
        {
            result[i] = v2result[i];
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
    public static float CounterClockWise(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v1 = b - a;
        Vector2 v2 = c - b;

        return v1.x * v2.y - v2.x * v1.y;
    }

    /// <summary>
    /// 선의 길이가 모두 1인 세 점 a-b-c를 잇는 꺾은선의 꺾인 방향 구하기
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns>
    /// 왼쪽 = 양수<br/>
    /// 오른쪽 = 음수<br/>
    /// 직선 = 0
    /// </returns>
    public static float CounterClockWiseNormal(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v1 = (b - a).normalized;
        Vector2 v2 = (c - b).normalized;

        return v1.x * v2.y - v2.x * v1.y;
    }
}