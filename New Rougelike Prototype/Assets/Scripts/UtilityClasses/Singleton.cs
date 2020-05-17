using UnityEngine;

//Allows any script that can be attached to a GameObject to become a singleton
//IF THERE ARE ISSUES BETWEEN SCENES, add DontDestroyOnLoad() functinoality!!
public class Singleton<T> : MonoBehaviour where T: MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if(_instance == null)
                _instance = (T)FindObjectOfType(typeof(T));
            if(_instance == null)
            {
                GameObject obj = new GameObject();
                _instance = obj.AddComponent<T>();
                obj.name = typeof(T).ToString();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        //Prevent duplicates
        if (_instance != null)
        {
            Debug.Log("Destroying duplicate singleton instance");
            Destroy(this.gameObject);
        }
    }



}
