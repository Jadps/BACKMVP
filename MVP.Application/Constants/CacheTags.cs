using System;

namespace MVP.Application.Constants;

public static class CacheTags
{
    public const string GlobalRoles = "global_roles";
    public const string AllMenus = "all_menus";
    public const string ModulesCache = "modules_cache";
    public const string SuperAdminMenus = "superadmin_menus";

    public static string UserMenu(string userId) => $"user_{userId}_menu";
    public static string UserMenu(Guid userUid) => $"user_{userUid}_menu";
    public static string TenantMenus(int tenantId) => $"tenant_{tenantId}_menus";
}

public static class CacheKeys
{
    public static string UserMenu(string userId) => $"menu_user_{userId}";
}
