using ILRuntime.Runtime.Enviorment;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestBundle : MonoBehaviour
{
    public GameObject c1;
    public GameObject c2;
    public GameObject sp1;
    public GameObject sp2;
    // Start is called before the first frame update
    void Start()
    {
        AssetBundleManager.serverURL = "http://10.1.23.61:80"; 
        //CheckData();
    }
    void CheckData()
    {
        StartCoroutine(AssetBundleManager.Instance.CheckUpgrade(ret =>
        {
            Debug.Log("资源检查");
            if (ret)
            {
                AssetBundleManager.Instance.Init(() =>
                {
                    Debug.Log("资源检查完毕");
                });
            }
        }
           ));
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            AssetBundleManager.Instance.LoadInstance<Transform>("cube", "Cube", trans=> { 
            if(trans!=null)
                {
                    c1 = trans.gameObject;
                    trans.SetParent(transform);
                }
            });
            AssetBundleManager.Instance.LoadInstance<Transform>("cb", "cb2", trans =>
            {
                if (trans != null)
                {
                    c2 = trans.gameObject;
                    trans.SetParent(transform);
                }
            });
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            AssetBundleManager.Instance.LoadInstance<Transform>("sp", "sp1", trans =>
            {
                if (trans != null)
                {
                    trans.SetParent(transform);
                    sp1 = trans.gameObject;
                }
            });
            AssetBundleManager.Instance.LoadInstance<Transform>("sp", "sp2", trans =>
            {
                if (trans != null)
                {
                    trans.SetParent(transform);
                    sp2 = trans.gameObject;
                }
            });
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(c1!=null)
            {
                DestoryAB(c1,"cb");
            }
            if (c2 != null)
            {
                DestoryAB(c2, "cb");
            }          
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (sp1 != null)
            {
                DestoryAB(sp1, "sp");
            }
            if (sp2 != null)
            {
                DestoryAB(sp2, "sp");
               
            }
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            //该函数的主要作用是查找并卸载不再使用的资源。游戏场景越复杂、资源越多，该函数的开销越大 
            Resources.UnloadUnusedAssets();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {

            InitILRunTimeTest();
        
        }
    }
    private void DestoryAB(GameObject go,string assetName)
    {
        GameObject.Destroy(go);
        AssetBundleManager.UnloadAssetBundle(assetName, true);
        Resources.UnloadUnusedAssets();
    }




    #region ILRuntimeTest
    AppDomain appdomain;

    void InitILRunTimeTest()
    {
        System.IO.MemoryStream fs = null;
        System.IO.MemoryStream p = null;
       
        AssetBundleManager.Instance.LoadResource<TextAsset>("hotfix_project", "HotFix_Project.dll", (textAsset) =>
        {

            Debug.Log("dll资源热更新  " + textAsset.bytes.Length);
            fs = new MemoryStream(textAsset.bytes);

            AssetBundleManager.Instance.LoadResource<TextAsset>("hotfix_project", "HotFix_Project.pdb", (_textAsset) =>
            {
                Debug.Log(_textAsset.bytes.Length);
                p = new MemoryStream(_textAsset.bytes);

                try
                {
                    appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
                    appdomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());

                }
                catch
                {

                    throw;
                }

                InitializeILRuntime();
                OnHotFixLoaded();


            });
        });
    }
    void InitializeILRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        //这里做一些ILRuntime的注册，HelloWorld示例暂时没有需要注册的
        appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
        appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
    }

    void OnHotFixLoaded()
    {
        //HelloWorld，第一次方法调用
        appdomain.Invoke("HotFix_Project.InstanceClass", "StaticFunTest", null, null);
        //appdomain.Invoke("HotFix_Project.InstanceClass", "GenericMethod", null, "123");
        appdomain.Invoke("HotFix_Project.TestValueType", "RunTest", null, null);

    }
    #endregion



    string textArea="http://10.1.23.61:80";
    private void OnGUI()
    {     
        textArea =GUI.TextArea(new Rect(new Vector2(0,100),new Vector2(Screen.width,100)),textArea);
        AssetBundleManager.serverURL = textArea;

        if (GUI.Button(new Rect(new Vector2(0, Screen.height / 2+200), new Vector2(Screen.width, 100)), " 检查资源更新"))
        {
            CheckData();
        }

        if (GUI.Button(new Rect(new Vector2(0,Screen.height/2),new Vector2(Screen.width,100))," ILRuntimeTest"))
        {
            InitILRunTimeTest();
        }
    }
}
