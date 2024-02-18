using CSTI_MiniLoader;
using MelonLoader;

[assembly: MelonInfo(typeof(MiniLoader), "CSTI_MiniLoader", MiniLoader.Version, "zender")]
[assembly: MelonGame("WinterSpring Games", "Card Survival - Tropical Island")]
[assembly: MelonGame("WinterSpringGames", "CardSurvivalTropicalIsland")]
[assembly: MelonGame("winterspringgames", "survivaljourney")]
[assembly: MelonGame("winterspringgames", "survivaljourneydemo")]
[assembly: MelonPlatform([MelonPlatformAttribute.CompatiblePlatforms.ANDROID])]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: HarmonyDontPatchAll]