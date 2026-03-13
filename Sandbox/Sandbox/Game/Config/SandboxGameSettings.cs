namespace Sandbox.Game.Config;

internal sealed class SandboxGameSettings
{
    public WindowSettings Window { get; set; } = WindowSettings.CreateDefault();

    public RenderSettings Render { get; set; } = RenderSettings.CreateDefault();

    public DebugSettings Debug { get; set; } = DebugSettings.CreateDefault();

    public InputSettings Input { get; set; } = InputSettings.CreateDefault();

    public SceneSettings Scene { get; set; } = SceneSettings.CreateDefault();

    public PlayerSettings Player { get; set; } = PlayerSettings.CreateDefault();

    public CameraSettings Camera { get; set; } = CameraSettings.CreateDefault();

    public DayNightSettings DayNight { get; set; } = DayNightSettings.CreateDefault();

    public CalendarSettings Calendar { get; set; } = CalendarSettings.CreateDefault();

    public NpcSystemSettings Npcs { get; set; } = NpcSystemSettings.CreateDefault();

    public DialogueSettings Dialogue { get; set; } = new();

    public InteractionSettings Interaction { get; set; } = InteractionSettings.CreateDefault();

    public MenuSettings Menu { get; set; } = MenuSettings.CreateDefault();

    public EconomySettings Economy { get; set; } = EconomySettings.CreateDefault();

    public ProgressionSettings Progression { get; set; } = ProgressionSettings.CreateDefault();

    public SleepSettings Sleep { get; set; } = SleepSettings.CreateDefault();

    public static SandboxGameSettings CreateDefault()
    {
        var settings = new SandboxGameSettings
        {
            Dialogue = DialogueSettingsLoader.LoadFromContent()
        };

        DialogueSettingsValidator.Validate(settings.Dialogue, settings.Npcs);
        return settings;
    }
}
