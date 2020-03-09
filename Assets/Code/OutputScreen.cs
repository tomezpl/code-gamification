using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutputScreen : MonoBehaviour
{
    Material mat;
    public RenderTexture rtt;
    public ProgramController program;
    public Text displayText;

    public bool showUserOutput = true;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.mainTexture = rtt;
        mat.SetTexture("_DetailAlbedoMap", rtt);
    }

    // Update is called once per frame
    void Update()
    {
        displayText.text = (showUserOutput) ? program.outputBuffer : program.expectedOutput;
    }
}
