# Scene Greybox Notes

These scenes are placeholders copied from the default Unity scene so the build flow exists early.

## SCN_MainRoom

P0 greybox objects:

- `Canvas_Root`
- `Panel_Newspaper`
- `Panel_YellowPages`
- `Button_OpenNewspaper`
- `Button_OpenYellowPages`
- `Button_DialSelectedMission`
- `HUD_Stress`
- `HUD_Cigarettes`

The scene should only call flow/controller methods. Do not mutate `PlayerState`, `MissionState`, or obituary data directly from UI buttons.

## SCN_Call

P0 greybox objects:

- `Canvas_Root`
- `Panel_NpcSilhouette`
- `Text_DialogueLine`
- `Group_ChoiceButtons`
- `HUD_Stress`
- `HUD_Breakdown`
- `HUD_CallCounter`
- `Button_UseCigarette`
- `Popup_Result`

Choice buttons should send choice IDs to the dialogue controller. Personality rules, dice checks, call counter updates, and ending priority stay in services.
