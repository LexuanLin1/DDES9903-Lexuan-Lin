using UnityEngine;

public sealed class RGBAuthenticationValidator : MonoBehaviour
{
    [Header("Output Renderers")]
    [SerializeField] private Renderer redOutput;
    [SerializeField] private Renderer greenOutput;
    [SerializeField] private Renderer blueOutput;

    [Header("Expected Colours")]
    [ColorUsage(true, true)]
    [SerializeField] private Color redTarget = Color.red;

    [ColorUsage(true, true)]
    [SerializeField] private Color greenTarget = Color.green;

    [ColorUsage(true, true)]
    [SerializeField] private Color blueTarget = Color.blue;

    [Header("Detection")]
    [SerializeField, Range(0.05f, 1f)]
    private float colourTolerance = 0.30f;

    [SerializeField, Min(0f)]
    private float minimumBrightness = 0.03f;

    [SerializeField, Min(0f)]
    private float requiredStableTime = 0.35f;

    [Header("Completion")]
    [SerializeField]
    private MissionSecurityController securityController;

    [SerializeField]
    private AudioSource successAudio;

    [SerializeField]
    private GameObject verifiedIndicator;

    [SerializeField]
    private GameObject[] objectsToDisableOnSuccess;

    private Material redMaterial;
    private Material greenMaterial;
    private Material blueMaterial;

    private float correctStateTimer;
    private bool authenticationCompleted;

    private void Awake()
    {
        redMaterial = GetRuntimeMaterial(redOutput);
        greenMaterial = GetRuntimeMaterial(greenOutput);
        blueMaterial = GetRuntimeMaterial(blueOutput);

        if (verifiedIndicator != null)
        {
            verifiedIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (authenticationCompleted)
        {
            return;
        }

        if (securityController == null ||
            !securityController.SecurityActive)
        {
            correctStateTimer = 0f;
            return;
        }

        bool redCorrect =
            MatchesTarget(redMaterial, redTarget);

        bool greenCorrect =
            MatchesTarget(greenMaterial, greenTarget);

        bool blueCorrect =
            MatchesTarget(blueMaterial, blueTarget);

        if (!redCorrect || !greenCorrect || !blueCorrect)
        {
            correctStateTimer = 0f;
            return;
        }

        correctStateTimer += Time.deltaTime;

        if (correctStateTimer >= requiredStableTime)
        {
            CompleteAuthentication();
        }
    }

    private void CompleteAuthentication()
    {
        if (authenticationCompleted)
        {
            return;
        }

        authenticationCompleted = true;

        if (securityController != null)
        {
            securityController.CompleteAuthentication();
        }

        if (verifiedIndicator != null)
        {
            verifiedIndicator.SetActive(true);
        }

        SetObjectsActive(objectsToDisableOnSuccess, false);

        if (successAudio != null &&
            successAudio.clip != null)
        {
            successAudio.Play();
        }

        Debug.Log(
            "RGB identity verification completed successfully.",
            this
        );
    }

    private Material GetRuntimeMaterial(Renderer targetRenderer)
    {
        if (targetRenderer == null)
        {
            return null;
        }

        return targetRenderer.material;
    }

    private bool MatchesTarget(
        Material material,
        Color expectedColour)
    {
        if (material == null)
        {
            return false;
        }

        Color actualColour = GetVisibleColour(material);

        float brightness = Mathf.Max(
            actualColour.r,
            actualColour.g,
            actualColour.b
        );

        if (brightness < minimumBrightness)
        {
            return false;
        }

        Vector3 actualNormalised =
            NormaliseRgb(actualColour);

        Vector3 expectedNormalised =
            NormaliseRgb(expectedColour);

        float difference = Vector3.Distance(
            actualNormalised,
            expectedNormalised
        );

        return difference <= colourTolerance;
    }

    private static Color GetVisibleColour(Material material)
    {
        if (material.HasProperty("_EmissionColor"))
        {
            Color emission =
                material.GetColor("_EmissionColor");

            if (Mathf.Max(
                    emission.r,
                    emission.g,
                    emission.b) > 0.001f)
            {
                return emission;
            }
        }

        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        if (material.HasProperty("_Color"))
        {
            return material.GetColor("_Color");
        }

        return Color.black;
    }

    private static Vector3 NormaliseRgb(Color colour)
    {
        float maximum = Mathf.Max(
            colour.r,
            colour.g,
            colour.b
        );

        if (maximum <= 0.0001f)
        {
            return Vector3.zero;
        }

        return new Vector3(
            colour.r / maximum,
            colour.g / maximum,
            colour.b / maximum
        );
    }

    private static void SetObjectsActive(
        GameObject[] targets,
        bool active)
    {
        if (targets == null)
        {
            return;
        }

        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
