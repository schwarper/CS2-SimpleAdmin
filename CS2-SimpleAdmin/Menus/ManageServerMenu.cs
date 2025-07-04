using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;

namespace CS2_SimpleAdmin.Menus;

public static class ManageServerMenu
{
    public static void OpenMenu(CCSPlayerController admin, BaseMenu prevMenu)
    {
        if (admin.IsValid == false)
            return;

        var localizer = CS2_SimpleAdmin._localizer;
        if (AdminManager.PlayerHasPermissions(new SteamID(admin.SteamID), "@css/generic") == false)
        {
            admin.PrintToChat(localizer?["sa_prefix"] ??
                              "[SimpleAdmin] " +
                              (localizer?["sa_no_permission"] ?? "You do not have permissions to use this command")
            );
            return;
        }

        var menu = AdminMenu.CreateMenu(localizer?["sa_menu_server_manage"] ?? "Server Manage");
        List<ChatMenuOptionData> options = [];


        // permissions
        var hasMap = AdminManager.CommandIsOverriden("css_map") ? AdminManager.PlayerHasPermissions(new SteamID(admin.SteamID), AdminManager.GetPermissionOverrides("css_map")) : AdminManager.PlayerHasPermissions(new SteamID(admin.SteamID), "@css/changemap");
        var hasPlugins = AdminManager.CommandIsOverriden("css_pluginsmanager") ? AdminManager.PlayerHasPermissions(new SteamID(admin.SteamID), AdminManager.GetPermissionOverrides("css_pluginsmanager")) : AdminManager.PlayerHasPermissions(new SteamID(admin.SteamID), "@css/root");

        //bool hasMap = AdminManager.PlayerHasPermissions(admin, "@css/changemap");

        // options added in order

        if (hasPlugins)
        {
            options.Add(new ChatMenuOptionData(localizer?["sa_menu_pluginsmanager_title"] ?? "Manage Plugins", () => admin.ExecuteClientCommandFromServer("css_pluginsmanager")));
        }

        if (hasMap)
        {
            options.Add(new ChatMenuOptionData(localizer?["sa_changemap"] ?? "Change Map", () => ChangeMapMenu(admin, menu)));
        }

        options.Add(new ChatMenuOptionData(localizer?["sa_restart_game"] ?? "Restart Game", () => CS2_SimpleAdmin.RestartGame(admin)));

        foreach (var menuOptionData in options)
        {
            var menuName = menuOptionData.Name;
            menu.AddItem(menuName, (_, _) => { menuOptionData.Action.Invoke(); }, menuOptionData.Disabled ? DisableOption.DisableHideNumber : DisableOption.None);
        }

        menu.PrevMenu = prevMenu;
        menu.Display(admin, 0);
    }

    private static void ChangeMapMenu(CCSPlayerController admin, BaseMenu prevMenu)
    {
        var menu = AdminMenu.CreateMenu(CS2_SimpleAdmin._localizer?["sa_changemap"] ?? "Change Map");
        List<ChatMenuOptionData> options = [];

        var maps = CS2_SimpleAdmin.Instance.Config.DefaultMaps;
        options.AddRange(maps.Select(map => new ChatMenuOptionData(map, () => ExecuteChangeMap(admin, map, false))));

        var wsMaps = CS2_SimpleAdmin.Instance.Config.WorkshopMaps;
        options.AddRange(wsMaps.Select(map => new ChatMenuOptionData($"{map.Key} (WS)", () => ExecuteChangeMap(admin, map.Value?.ToString() ?? map.Key, true))));

        foreach (var menuOptionData in options)
        {
            var menuName = menuOptionData.Name;
            menu.AddItem(menuName, (_, _) => { menuOptionData.Action.Invoke(); }, menuOptionData.Disabled ? DisableOption.DisableHideNumber : DisableOption.None);;
        }

        menu.PrevMenu = prevMenu;
        menu.Display(admin, 0);
    }

    private static void ExecuteChangeMap(CCSPlayerController admin, string mapName, bool workshop)
    {
        if (workshop)
            CS2_SimpleAdmin.Instance.ChangeWorkshopMap(admin, mapName);
        else
            CS2_SimpleAdmin.Instance.ChangeMap(admin, mapName);
    }
}