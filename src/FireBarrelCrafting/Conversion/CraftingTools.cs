using FireBarrelCrafting.Configuration;
using FireBarrelCrafting.Infrastructure;
using Il2Cpp;

namespace FireBarrelCrafting.Conversion;

/// <summary>Finds a usable tool in the player's inventory and wears it down.</summary>
/// <param name="settings">Supplies the allowed tools and the wear amount.</param>
/// <param name="log">Mod logger.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class CraftingTools(FireBarrelSettings settings, ModLog log)
{
    private readonly FireBarrelSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly ModLog _log = log ?? throw new ArgumentNullException(nameof(log));

    /// <summary>Gets a readable list of acceptable tools, e.g. "Hacksaw, Hatchet or Improvised Hatchet".</summary>
    public string RequirementText => ToolNames.DescribeAlternatives(_settings.AllowedTools);

    /// <summary>Picks the tool to use from the player's inventory.</summary>
    /// <returns>The best-condition allowed tool that is not ruined, or <see langword="null"/> if there is none.</returns>
    public GearItem? FindBest()
    {
        Inventory inventory = GameManager.GetInventoryComponent();
        if (inventory == null)
        {
            return null;
        }

        GearItem? best = null;
        foreach (string gearName in _settings.AllowedTools)
        {
            GearItem candidate = inventory.GetHighestConditionGearThatMatchesName(gearName);
            if (candidate == null || candidate.CurrentHP <= 0f)
            {
                continue;
            }

            if (best == null || candidate.GetNormalizedCondition() > best.GetNormalizedCondition())
            {
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>Reduces the tool's condition by the configured percentage of its max condition.</summary>
    /// <param name="tool">The tool that was used.</param>
    public void ApplyWear(GearItem tool)
    {
        float lossPercent = _settings.ToolConditionLossPercent;
        if (tool == null || lossPercent <= 0f)
        {
            return;
        }

        float loss = tool.GearItemData.MaxHP * lossPercent / 100f;
        tool.Degrade(loss);
        _log.Debug($"{tool.name} lost {loss:0.##} HP.");
    }
}
