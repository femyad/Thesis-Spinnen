using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ReadJson : MonoBehaviour
{



    public TextAsset textJSON;


    [System.Serializable]
    public class SpiderData
    {
        public GeneralSpider generalSpider;
    }

    [System.Serializable]
    public class GeneralSpider
    {
        public string name;
        public Prosoma prosoma;

    }

    [System.Serializable]
    public class Prosoma
    {
        public Dimensions dimensions;
        public Chelicerae chelicerae;
        public Pedipalps pedipalps;
        public Eyes eyes;
        public Legs legs;
    }



    [System.Serializable]
    public class Dimensions
    {
        public float width;
        public float length;
        public float height;
        public float size;
    }



    [System.Serializable]
    public class Chelicerae
    {
        public bool fangs;
        public float fangsSize;
        public string movement;
        public Dimensions dimensions;
    }



    [System.Serializable]
    public class Pedipalps
    {
        public Dimensions dimensions;
    }

    [System.Serializable]
    public class  Eyes
    {
        public int numberOfEyes;
        public float eyeSize;
        public string eyesArrangement;
    }

    [System.Serializable]
    public class Legs
    {
        public LegL1 legL1;
    }

    [System.Serializable]
    public class LegL1
    {
        public Coxa coxa;
        public Trochanter trochanter;
        
    }
    
    [System.Serializable]
    public class SpecialElements
    {
        public string coxaType;
    }

    

    [System.Serializable]
    public class Coxa
    {
        public SpecialElements specialElements;
        public float width;
        public float length;
        public float height;
        public float size;
        public string shape;
        public string idleOrientation;
        public string maxOrientation;
        public string minOrientation;
    }

    [System.Serializable]
    public class Trochanter 
    {
        public float width;
        public float length;
        public float height;
        public float size;
        public string shape;
        public string idleOrientation;
        public string maxOrientation;
        public string minOrientation;
    }






    public SpiderData spiderData = new();

    // Start is called before the first frame update
    void Start()
    {
        spiderData = JsonUtility.FromJson<SpiderData>(textJSON.text);
        Debug.Log("Spider Name: " + spiderData.generalSpider.name);
        // Debug.Log("Prosoma Width: " + spiderData.generalSpider.prosoma.width);
        // Debug.Log("Chelicerae Fangs: " + spiderData.generalSpider.chelicerae.fangs);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
