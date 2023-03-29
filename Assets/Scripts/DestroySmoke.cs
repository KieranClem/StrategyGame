using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySmoke : MonoBehaviour
{
    
    public void StartDestruction(float time)
    {
        StartCoroutine(removeSmoke(time));
    }
    
    public IEnumerator removeSmoke(float TimeToWait)
    {
        yield return new WaitForSeconds(TimeToWait);
        Destroy(this.gameObject);
    }
}
