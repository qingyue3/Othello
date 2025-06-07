using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField]
    private Color normalcolor;

    [SerializeField]
    private Color mouseOvercolor;

    private Material material;

    // Start is called before the first frame update
    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        material.color = normalcolor;
    }

    private void OnMouseEnter()
    {
        material.color = mouseOvercolor;
    }

    private void OnMouseExit()
    {
        material.color = normalcolor;
    }

    private void OnDestroy()
    {
        Destroy(material);
    }

}
