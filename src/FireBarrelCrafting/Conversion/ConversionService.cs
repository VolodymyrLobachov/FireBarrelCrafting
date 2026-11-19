using System.Diagnostics.CodeAnalysis;
using FireBarrelCrafting.Configuration;
using FireBarrelCrafting.Infrastructure;
using FireBarrelCrafting.Spawning;
using Il2Cpp;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace FireBarrelCrafting.Conversion;

/// <summary>
/// Runs one conversion: checks requirements, shows the progress bar, then swaps the source decoration (barrel, wheel)
/// for the chosen vanilla fire (GRASP: Controller). The actual work is delegated to collaborators.
/// </summary>
/// <param name="settings">Supplies the craft duration.</param>
/// <param name="detector">Re-checks the source when the progress bar finishes.</param>
/// <param name="tools">Finds and wears the tool.</param>
/// <param name="factory">Creates the fire barrel or rim grill.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class ConversionService(
    FireBarrelSettings settings,
    SourceDetector detector,
    CraftingTools tools,
    FireFactory factory,
    ModLog log)
{
    private const float ProgressBarSeconds = 5f;

    private readonly FireBarrelSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly SourceDetector _detector = detector ?? throw new ArgumentNullException(nameof(detector));
    private readonly CraftingTools _tools = tools ?? throw new ArgumentNullException(nameof(tools));
    private readonly FireFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    private PendingConversion? _pending;

    /// <summary>
    /// Starts converting <paramref name="source"/>: shows a HUD message if a requirement is missing,
    /// otherwise launches the progress bar. Ignored while another conversion is running.
    /// </summary>
    /// <param name="source">A decoration accepted by <see cref="SourceDetector.IsConvertible"/>.</param>
    /// <param name="target">What to turn the source into.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    public void Begin(DecorationItem source, ConversionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (_pending is not null || source == null)
        {
            return;
        }

        if (!_detector.IsConvertible(source, target))
        {
            // The entry was added before a setting changed (e.g. rim grills from barrels turned off).
            _log.Debug($"'{source.name}' can no longer become {target.Noun}.");
            return;
        }

        if (!TryCheckRequirements(source, target, out GearItem? tool, out string? problem))
        {
            HUDMessage.AddMessage(problem, false, false);
            return;
        }

        _pending = new PendingConversion(source, target, tool);
        _log.Debug($"Starting conversion of '{source.name}' into {target.Noun} with {tool.name}.");

        var onExit = DelegateSupport.ConvertDelegate<OnExitDelegate>(new Action<bool, bool, float>(OnProgressBarExit));
        InterfaceManager.GetPanel<Panel_GenericProgressBar>().Launch(
            target.Progress, ProgressBarSeconds, _settings.CraftMinutes, randomFailureThreshold: 0f,
            audioName: BreakDownAudioOf(source), voiceName: null,
            supressHeavyBreathing: false, skipRestoreInHands: false, del: onExit);
    }

    private bool TryCheckRequirements(
        DecorationItem source,
        ConversionTarget target,
        [NotNullWhen(true)] out GearItem? tool,
        [NotNullWhen(false)] out string? problem)
    {
        tool = null;
        problem = null;

        if (source.HasChildren())
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

            if (!_detector.IsConvertible(pending.Source, pending.Target))
            {
                // Removed meanwhile, or a setting changed during the craft (e.g. rim grills from barrels turned off).
                _log.Warning($"'{pending.Source.name}' can no longer become {pending.Target.Noun}; nothing was crafted.");
                return;
            }

            Replace(pending.Source, pending.Target);
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
        GameObject source = decoration.gameObject;
        string sourceName = source.name;
        Vector3 position = source.transform.position;
        // Keep it upright even if the source was tilted (a wheel may lie on its side).
        Quaternion rotation = Quaternion.Euler(0f, source.transform.eulerAngles.y, 0f);
        // Source and target prefabs have different pivots, so the new object is aligned by its bottom.
        float? floorHeight = GroundAligner.TryGetBottom(source, out float bottom) ? bottom : null;

        SourceRemover.Remove(decoration);

        string guid = _factory.Create(target.PrefabAddress, position, rotation, floorHeight);
        _log.Info($"Converted '{sourceName}' into {target.Noun} at {position} (guid {guid}).");
    }

    private static string? BreakDownAudioOf(DecorationItem source)
    {
        BreakDown breakDown = source.GetComponent<BreakDown>();
        if (breakDown == null)
        {
            return null;
        }

        string audio = breakDown.m_BreakDownAudio;
        return string.IsNullOrEmpty(audio) ? null : audio;
    }

    /// <summary>The conversion waiting for the progress bar to finish.</summary>
    /// <param name="Source">Decoration being converted.</param>
    /// <param name="Target">What it becomes.</param>
    /// <param name="Tool">Tool to wear when it succeeds.</param>
    private sealed record PendingConversion(DecorationItem Source, ConversionTarget Target, GearItem Tool);
}
