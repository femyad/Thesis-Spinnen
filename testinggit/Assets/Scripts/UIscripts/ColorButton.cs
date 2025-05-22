using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public Color colorToApply;                  // Set in Inspector
    public PatellaScaler patellaScalerTarget;   // Drag the PatellaScaler (on Canvas)

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (patellaScalerTarget != null)
            {
                patellaScalerTarget.SetColor(colorToApply);
            }
        });
    }
}
