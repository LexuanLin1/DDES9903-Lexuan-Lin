using System.Collections;
using UnityEngine;

public sealed class SequentialColourStageValidator : MonoBehaviour
{
    private static readonly int BaseColourProperty =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColourProperty =
        Shader.PropertyToID("_Color");

    private static readonly int EmissionColourProperty =
        Shader.PropertyToID("_EmissionColor");

    [Header("Sequence")]
    public SequentialIdentityAuthentication identitySequence;
    public IdentityColourStage stage;

    [Header("Colour Output")]
    public Renderer outputRenderer;

    [Header("Dominant Channel Detection")]
    [Range(0.001f, 1f)]
    public float minimumBrightness = 0.01f;

    [Range(0.001f, 0.5f)]
    public float minimumDominance = 0.01f;

    [Min(0f)]
    public float validationDelay = 0.08f;

    public bool showDetectedColourInConsole = true;

    private bool validationPending;

    public void ValidateCurrentColour()
    {
        if (validationPending ||
            identitySequence == null ||
            outputRenderer == null)
        {
            return;
        }

        if (!identitySequence.CanValidateStage(stage))
        {
            return;
        }

        StartCoroutine(ValidateAfterColourUpdate());
    }

    private IEnumerator ValidateAfterColourUpdate()
    {
        validationPending = true;

        yield return null;

        if (validationDelay > 0f)
        {
            yield return new WaitForSeconds(
                validationDelay
            );
        }

        if (identitySequence == null ||
            outputRenderer == null ||
            !identitySequence.CanValidateStage(stage))
        {
            validationPending = false;
            yield break;
        }

        Color detectedColour =
            ReadBestAvailableColour();

        if (showDetectedColourInConsole)
        {
            Debug.Log(
                "Detected colour for " +
                stage +
                ": R=" +
                detectedColour.r.ToString("F3") +
                ", G=" +
                detectedColour.g.ToString("F3") +
                ", B=" +
                detectedColour.b.ToString("F3"),
                this
            );
        }

        bool correct =
            IsCorrectDominantColour(
                detectedColour
            );

        validationPending = false;

        if (correct)
        {
            ReportCorrectStage();
        }
        else
        {
            identitySequence.ReportIncorrectAttempt();
        }
    }

    private Color ReadBestAvailableColour()
    {
        Material currentMaterial =
            outputRenderer.material;

        Color baseColour = Color.black;
        Color emissionColour = Color.black;

        if (currentMaterial.HasProperty(
                BaseColourProperty))
        {
            baseColour = currentMaterial.GetColor(
                BaseColourProperty
            );
        }
        else if (currentMaterial.HasProperty(
                     ColourProperty))
        {
            baseColour = currentMaterial.GetColor(
                ColourProperty
            );
        }

        if (currentMaterial.HasProperty(
                EmissionColourProperty))
        {
            emissionColour =
                currentMaterial.GetColor(
                    EmissionColourProperty
                );
        }

        float baseStrength =
            GetColourStrength(baseColour);

        float emissionStrength =
            GetColourStrength(emissionColour);

        return emissionStrength > baseStrength
            ? emissionColour
            : baseColour;
    }

    private bool IsCorrectDominantColour(
        Color colour)
    {
        float highestChannel = Mathf.Max(
            colour.r,
            colour.g,
            colour.b
        );

        if (highestChannel < minimumBrightness)
        {
            return false;
        }

        float red =
            colour.r / highestChannel;

        float green =
            colour.g / highestChannel;

        float blue =
            colour.b / highestChannel;

        switch (stage)
        {
            case IdentityColourStage.Red:
                return red >=
                           green + minimumDominance &&
                       red >=
                           blue + minimumDominance;

            case IdentityColourStage.Green:
                return green >=
                           red + minimumDominance &&
                       green >=
                           blue + minimumDominance;

            case IdentityColourStage.Blue:
                return blue >=
                           red + minimumDominance &&
                       blue >=
                           green + minimumDominance;

            default:
                return false;
        }
    }

    private void ReportCorrectStage()
    {
        switch (stage)
        {
            case IdentityColourStage.Red:
                identitySequence.ReportRedCorrect();
                break;

            case IdentityColourStage.Green:
                identitySequence.ReportGreenCorrect();
                break;

            case IdentityColourStage.Blue:
                identitySequence.ReportBlueCorrect();
                break;
        }
    }

    private static float GetColourStrength(
        Color colour)
    {
        float highestChannel = Mathf.Max(
            colour.r,
            colour.g,
            colour.b
        );

        float lowestChannel = Mathf.Min(
            colour.r,
            colour.g,
            colour.b
        );

        return highestChannel - lowestChannel;
    }
}