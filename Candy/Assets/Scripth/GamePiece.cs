using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int cordenadaX;
    public int cordenadaY;

    Vector3 startPos;
    Vector3 endPos;
    [Range(0f, 1f)] float t;

    public void Cordenada(int x, int y)
    {
        cordenadaX = x;
        cordenadaY = y;
    }

    private void Update()
    {
        transform.position = startPos;
        Vector3.Lerp(startPos, endPos, t);
    }
    /*IEnumerable Corrutine()
    {
        while()
        {

        }
        yield return WaitForSeconds(1);
    }*/
}
