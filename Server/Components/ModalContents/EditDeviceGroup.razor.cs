using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Remotely.Server.Services;
using Remotely.Shared.Entities;

namespace Remotely.Server.Components.ModalContents;

[Authorize]
public partial class EditDeviceGroup : AuthComponentBase
{
    public static string DeviceGroupsPropName => nameof(DeviceGroups);
    public static string EditUserPropName => nameof(EditUser);
    [Parameter]
    public required DeviceGroup[] DeviceGroups { get; set; }

    [Parameter]
    public required RemotelyUser EditUser { get; set; }

    [Inject]
    private IDataService DataService { get; init; } = null!;

    [Inject]
    private IToastService ToastService { get; init; } = null!;


    private HashSet<string>? _memberGroupIds;

    // Tracked locally so the checkboxes stay in sync after changes made in this modal.
    private HashSet<string> MemberGroupIds => _memberGroupIds ??=
        (DeviceGroups ?? Array.Empty<DeviceGroup>())
            .Where(x => x.Users.Any(u => u.Id == EditUser.Id))
            .Select(x => x.ID)
            .ToHashSet();

    private bool AllGroupsSelected =>
        DeviceGroups?.Length > 0 &&
        DeviceGroups.All(x => MemberGroupIds.Contains(x.ID));

    private bool DoesGroupContainUser(DeviceGroup group)
    {
        return MemberGroupIds.Contains(group.ID);
    }

    private async Task GroupCheckChanged(ChangeEventArgs args, DeviceGroup group)
    {
        if (!string.IsNullOrWhiteSpace(EditUser.UserName) &&
            args.Value is bool boolValue &&
            boolValue)
        {
            if (!DataService.AddUserToDeviceGroup(EditUser.OrganizationID, group.ID, EditUser.UserName, out var result))
            {
                ToastService.ShowToast(result, classString: "bg-warning");
            }
            else
            {
                MemberGroupIds.Add(group.ID);
                ToastService.ShowToast("User added to group.");
            }

        }
        else
        {
            var result = await DataService.RemoveUserFromDeviceGroup(EditUser.OrganizationID, group.ID, EditUser.Id);
            if (!result)
            {
                ToastService.ShowToast("Failed to remove from group.", classString: "bg-warning");
            }
            else
            {
                MemberGroupIds.Remove(group.ID);
                ToastService.ShowToast("Removed user from group.");
            }
        }
    }

    private async Task SelectAllChanged(ChangeEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(EditUser.UserName))
        {
            return;
        }

        var select = args.Value is bool boolValue && boolValue;
        var succeeded = 0;
        var failed = 0;

        foreach (var group in DeviceGroups ?? Array.Empty<DeviceGroup>())
        {
            var isMember = MemberGroupIds.Contains(group.ID);
            if (isMember == select)
            {
                continue;
            }

            if (select)
            {
                if (DataService.AddUserToDeviceGroup(EditUser.OrganizationID, group.ID, EditUser.UserName, out _))
                {
                    MemberGroupIds.Add(group.ID);
                    succeeded++;
                }
                else
                {
                    failed++;
                }
            }
            else
            {
                if (await DataService.RemoveUserFromDeviceGroup(EditUser.OrganizationID, group.ID, EditUser.Id))
                {
                    MemberGroupIds.Remove(group.ID);
                    succeeded++;
                }
                else
                {
                    failed++;
                }
            }
        }

        var action = select ? "added to" : "removed from";
        if (failed > 0)
        {
            ToastService.ShowToast($"User {action} {succeeded} groups, {failed} failed.", classString: "bg-warning");
        }
        else
        {
            ToastService.ShowToast($"User {action} {succeeded} groups.");
        }
    }
}
