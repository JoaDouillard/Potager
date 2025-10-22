using System.Collections;
using UnityEngine;

public class Grossir : MonoBehaviour
{

    public float speedGrossir = 1f;
    public float timeBeforeScale = 2f;

    public float currentTimeBeforeScale = 2f;
    public float maxScale = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Coroutine_ScaleUp(transform);


    }

    // Update is called once per frame
    void Update()
    {
        while (true)
        {
            ScaleUp();
        }
    }
    
    private void ScaleUp()
    {
        transform.localScale += Vector3.one; 
    }

    private IEnumerator Coroutine_ScaleUp(Transform me)
    {
        Debug.Log("je suis là");
        while (me.localScale.x < maxScale)
        {
            yield return new WaitForEndOfFrame();
            me.localScale += Vector3.one * 1.0f;
            Debug.Log("je suis plus là");
        }
        oui();

    }

    private void oui()
    {
        Debug.Log("oui");
    }
}
