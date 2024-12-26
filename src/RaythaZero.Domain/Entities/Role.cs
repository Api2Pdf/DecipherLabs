﻿namespace RaythaZero.Domain.Entities;
public class Role : BaseAuditableEntity
{
    public string Label { get; set; } = null!;
    public string DeveloperName { get; set; } = null!;
    public SystemPermissions SystemPermissions { get; set; }
    public virtual ICollection<User> Users { get; set; }
}

public class BuiltInRole : ValueObject
{
    static BuiltInRole()
    {
    }

    private BuiltInRole()
    {
    }

    private BuiltInRole(string label, string developerName, SystemPermissions permission)
    {
        DefaultLabel = label;
        DeveloperName = developerName;
        DefaultSystemPermission = permission;
    }

    public static BuiltInRole From(string developerName)
    {
        var type = Permissions.FirstOrDefault(p => p.DeveloperName == developerName);

        if (type == null)
        {
            throw new Exception(developerName);
        }

        return type;
    }

    public static BuiltInRole SuperAdmin => new("Super Admin", "super_admin", BuiltInSystemPermission.AllPermissionsAsEnum);
    public static BuiltInRole Admin => new("Admin", "admin", BuiltInSystemPermission.AllPermissionsAsEnum & ~SystemPermissions.ManageAdministrators);
    public static BuiltInRole Editor => new("Editor", "editor", SystemPermissions.None);

    public string DefaultLabel { get; private set; } = string.Empty;
    public string DeveloperName { get; private set; } = string.Empty;
    public SystemPermissions DefaultSystemPermission { get; private set; } = SystemPermissions.None;

    public static implicit operator string(BuiltInRole scheme)
    {
        return scheme.DeveloperName;
    }

    public static explicit operator BuiltInRole(string type)
    {
        return From(type);
    }

    public override string ToString()
    {
        return DeveloperName;
    }

    public static IEnumerable<BuiltInRole> Permissions
    {
        get
        {
            yield return SuperAdmin;
            yield return Admin;
            yield return Editor;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DeveloperName;
    }
}


[Flags]
public enum SystemPermissions
{
    None = 0,
    ManageSystemSettings = 1,
    ManageAuditLogs = 2,
    ManageAdministrators = 4,
    ManageUsers = 8 
}

public class BuiltInSystemPermission : ValueObject
{
    public const string MANAGE_USERS_PERMISSION = "users";
    public const string MANAGE_ADMINISTRATORS_PERMISSION = "administrators";
    public const string MANAGE_AUDIT_LOGS_PERMISSION = "audit_logs";
    public const string MANAGE_SYSTEM_SETTINGS_PERMISSION = "system_settings";

    static BuiltInSystemPermission()
    {
    }

    private BuiltInSystemPermission()
    {
    }

    private BuiltInSystemPermission(string label, string developerName, SystemPermissions permission)
    {
        Label = label;
        DeveloperName = developerName;
        Permission = permission;
    }

    public static BuiltInSystemPermission From(string developerName)
    {
        var type = Permissions.FirstOrDefault(p => p.DeveloperName == developerName);

        if (type == null)
        {
            throw new NotImplementedException(developerName);
        }

        return type;
    }

    public static IEnumerable<BuiltInSystemPermission> From(SystemPermissions permission)
    {
        var permissions = new List<BuiltInSystemPermission>();

        if (permission.HasFlag(SystemPermissions.ManageAuditLogs))
            permissions.Add(ManageAuditLogs);
        if (permission.HasFlag(SystemPermissions.ManageAdministrators))
            permissions.Add(ManageAdministrators);
        if (permission.HasFlag(SystemPermissions.ManageSystemSettings))
            permissions.Add(ManageSystemSettings);
        if (permission.HasFlag(SystemPermissions.ManageUsers))
            permissions.Add(ManageUsers);
        return permissions;
    }

    public static SystemPermissions From(params string[] developerNames)
    {
        SystemPermissions builtPermission = SystemPermissions.None;
        foreach (string developerName in developerNames)
        {
            var type = Permissions.FirstOrDefault(p => p.DeveloperName == developerName);

            if (type == null)
            {
                throw new NotImplementedException(developerName);
            }

            builtPermission = builtPermission | type.Permission;
        }
        return builtPermission;
    }

    public static BuiltInSystemPermission ManageSystemSettings => new("Manage System Settings", MANAGE_SYSTEM_SETTINGS_PERMISSION, SystemPermissions.ManageSystemSettings);
    public static BuiltInSystemPermission ManageAuditLogs => new("Manage Audit Logs", MANAGE_AUDIT_LOGS_PERMISSION, SystemPermissions.ManageAuditLogs);
    public static BuiltInSystemPermission ManageAdministrators => new("Manage Administrators", MANAGE_ADMINISTRATORS_PERMISSION, SystemPermissions.ManageAdministrators);
    public static BuiltInSystemPermission ManageUsers => new("Manage Users", MANAGE_USERS_PERMISSION, SystemPermissions.ManageUsers);

    public string Label { get; private set; } = string.Empty;
    public string DeveloperName { get; private set; } = string.Empty;
    public SystemPermissions Permission { get; private set; } = SystemPermissions.None;

    public static implicit operator SystemPermissions(BuiltInSystemPermission scheme)
    {
        return scheme.Permission;
    }

    public static explicit operator BuiltInSystemPermission(string type)
    {
        return From(type);
    }

    public override string ToString()
    {
        return DeveloperName;
    }

    public static IEnumerable<BuiltInSystemPermission> Permissions
    {
        get
        {
            yield return ManageSystemSettings;
            yield return ManageAuditLogs;
            yield return ManageAdministrators;
            yield return ManageUsers;
        }
    }

    public static SystemPermissions AllPermissionsAsEnum
    {
        get
        {
            return
                ManageSystemSettings.Permission |
                ManageAuditLogs.Permission |
                ManageAdministrators.Permission |
                ManageUsers.Permission;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DeveloperName;
    }
}