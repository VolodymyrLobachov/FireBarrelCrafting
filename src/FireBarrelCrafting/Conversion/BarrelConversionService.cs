using System.Diagnostics.CodeAnalysis;
using FireBarrelCrafting.Configuration;
using FireBarrelCrafting.Infrastructure;
using FireBarrelCrafting.Spawning;
using Il2Cpp;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace FireBarrelCrafting.Conversion;

/// <summary>
/// Runs one conversion: checks requirements, shows the progress bar, then swaps the decorative barrel
/// for the chosen vanilla fire (GRASP: Controller). The actual work is delegated to collaborators.
/// </summary>
/// <param name="settings">Supplies the craft duration.</param>
/// <param name="detector">Re-checks the barrel when the progress bar finishes.</param>
/// <param name="tools">Finds and wears the tool.</param>
/// <param name="factory">Creates the fire barrel or rim grill.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class BarrelConversionService(
    FireBarrelSettings settings,
    BarrelDetector detector,
    CraftingTools tools,
    FireFactory factory,
    ModLog log)
{
    private const float ProgressBarSeconds = 5f;

    private readonly FireBarrelSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly BarrelDetector _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    private readonly CraftingTools _tools = tools ?? throw new ArgumentNullException(nameof(tools));
    private readonly FireFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    private PendingConversion? _pending;

    /// <summary>
    /// Starts converting <paramref name="barrel"/>: shows a HUD message if a requirement is missing,
    /// otherwise launches the progress bar. Ignored while another conversion is running.
    /// </summary>
    /// <param name="barrel">A barrel accepted by <see cref="BarrelDetector.IsConvertible"/>.</param>
    /// <param name="target">What to turn the barrel into.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    public void Begin(DecorationItem barrel, ConversionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (_pending is not null || barrel == null)
        {
            return;
        }

        if (!TryCheckRequirements(barrel, target, out GearItem? tool, out string? problem))
        {
            HUDMessage.AddMessage(problem, false, false);
            return;
        }

        _pending = new PendingConversion(barrel, target, tool);
        _log.Debug($"Starting conversion of '{barrel.name}' into {target.Noun} with {tool.name}.");

        var onExit = DelegateSupport.ConvertDelegate<OnExitDelegate>(new Action<bool, bool, float>(OnProgressBarExit));
        InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(
            target.Progress, ProgressBarSeconds, _settings.CraftMinutes, randomFailureThreshold: 0f,
            audioName: BreakDownAudioOf(barrel), voiceName: null,
            supressHeavyBreathing: false, skipRestoreInHands: false, del: onExit);
    }

    private bool TryCheckRequirements(
        DecorationItem barrel,
        ConversionTarget target,
        [NotNullWhen(true)] out GearItem? tool,
        [NotNullWhen(false)] out string? problem)
    {
        tool = null;
        problem = null;

        if (barrel.HasChildren())
        {
            problem = ModText.RemoveItemsFirst;
            return false;
        }

        tool = _tools.FindBest();
        if (tool == null)
        {
            problem = target.ToolRequired(_tools.RequirementText);
            return false;
        }

        if (!_factory.IsPrefabAvailable(target.PrefabAddress))
        {
            tool = null;
            problem = target.PrefabMissing;
            return false;
        }

        return true;
    }

    private void OnProgressBarExit(bool success, bool playerCancel, float progress)
    {
        PendingConversion? pending = _pending;
        _pending = null;

        try
        {
            if (pending is null || !success || playerCancel)
            {
                _log.Debug($"Conversion cancelled (success={success}, cancel={playerCancel}, progress={progress:0.00}).");
                return;
            }

            if (!_detector.IsConvertible(pending.Barrel))
            {
                _log.Warning("The barrel disappeared before the conversion finished.");
                return;
            }

            Replace(pending.Barrel, pending.Target);
            _tools.ApplyWear(pending.Tool);
            HUDMessage.AddMessage(pending.Target.Success, false, false);
        }
        catch (Exception e)
        {
            // Called from the game's UI; an escaping exception would leave the progress bar in a broken state.
            _log.Error("Finishing the conversion failed", e);
        }
    }

    private void Replace(DecorationItem decoration, ConversionTarget target)
    {
        GameObject barrel = decoration.gameObject;
        string barrelName = barrel.name;
        Vector3 position = barrel.transform.position;
        // Keep it upright even if the decorative barrel was slightly tilted.
        Quaternion rotation = Quaternion.Euler(0f, barrel.transform.eulerAngles.y, 0f);
        // The two prefabs have different pivots, so the new object is aligned by its bottom.
        float? floorHeight = GroundAligner.TryGetBottom(barrel, out float bottom) ? bottom : null;

        // Remove the decorative barrel the vanilla way (the game saves it by guid). No yield: it became the new object.
        barrel.GetComponent<BreakDown>().DoBreakDown(false);

        string guid = _factory.Create(target.PrefabAddress, position, rotation, floorHeight);
        _log.Info($"Converted '{barrelName}' into {target.Noun} at {position} (guid {guid}).");
    }

    private static string? BreakDownAudioOf(DecorationItem barrel)
    {
        string audio = barrel.GetComponent<BreakDown>().m_BreakDownAudio;
        return string.IsNullOrEmpty(audio) ? null : audio;
    }

    /// <summary>The conversion waiting for the progress bar to finish.</summary>
    /// <param name="Barrel">Barrel being converted.</param>
    /// <param name="Target">What it becomes.</param>
    /// <param name="Tool">Tool to wear when it succeeds.</param>
    private sealed record PendingConversion(DecorationItem Barrel, ConversionTarget Target, GearItem Tool);
}
