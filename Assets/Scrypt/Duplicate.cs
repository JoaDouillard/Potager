using UnityEngine;


public class Duplicate : MonoBehaviour
{
    public GameObject Object;
    public float Timer = 1f;
    public float CurrentTimer = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void ObjectCreate()
    {
        Instantiate(Object, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (Timer <= 0)
        {
            ObjectCreate();
            Timer = CurrentTimer;
        }
        else
        {
            Timer -= Time.deltaTime;
        }
    }
}
