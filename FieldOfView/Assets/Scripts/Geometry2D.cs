using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public static class Geometry2D
{
    /// <summary>
    /// 선분 a-b와 c-d의 교차 판정
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    public static bool IsCross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float ccw1 = CounterClockWise(a, b, c) * CounterClockWise(a, b, d);
        float ccw2 = CounterClockWise(c, d, a) * CounterClockWise(c, d, b);

        if (ccw1 == 0f && ccw2 == 0f)
        {
            if ((a.x, a.y).CompareTo((b.x, b.y)) > 0)
            {
                (a, b) = (b, a);
            }

            if ((c.x, c.y).CompareTo((d.x, d.y)) > 0)
            {
                (c, d) = (d, c);
            }

            return (c.x, c.y).CompareTo((b.x, b.y)) <= 0 && (a.x, a.y).CompareTo((d.x, d.y)) <= 0;
        }

        return ccw1 <= 0f && ccw2 <= 0f;
    }

    /// <summary>
    /// 단순 다각형 판별하기
    /// </summary>
    /// <param name="vertices"> 다각형을 이루는 정렬된 점들</param>
    /// <returns></returns>
    public static bool IsSimplyPolygon(Vector2[] vertices)
    {
        int len = vertices.Length;
        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < len; j++)
            {
                // 자기자신 또는 이웃한 선분은 비교 X
                if (i == j || (i + 1) % len == j || (j + 1) % len == i)
                {
                    continue;
                }

                if (IsCross(vertices[i], vertices[(i + 1) % len], vertices[j], vertices[(j + 1) % len]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 볼록 다각형 판별하기
    /// </summary>
    /// <param name="vertices"> 단순 다각형을 이루는 점들 </param>
    /// <returns></returns>
    public static bool IsConvexPolygon(Vector2[] vertices)
    {
        if (vertices.Length < 2)
        {
            throw new ArgumentOutOfRangeException("Too few vertices");
        }

        bool isSign = CounterClockWise(vertices[0], vertices[1], vertices[2]) < 0f;
        int len = vertices.Length;
        for (int i = 1; i < len; i++)
        {
            if (CounterClockWise(vertices[i], vertices[(i + 1) % len], vertices[(i + 2) % len]) < 0f != isSign)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 오목 다각형을 여러개의 볼록 다각형으로 분할하기
    /// </summary>
    /// <param name="vertices"> 단순 다각형을 이루는 점들 </param>
    /// <param name="results"> 볼록 다각형들 </param>
    /// <param name="counts"> 볼록 다각형의 점의 개수들 </param>
    /// <returns>볼록 다각형의 수</returns>
    [Obsolete("", true)]
    public static int Concave2Convex(Vector2[] vertices, Vector2[][] results, int[] counts)
    {
        throw new NotImplementedException();
    }

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