using Identity.Domain.Constants;

namespace Identity.Domain.Configuration;

public static class DefaultRolePermissions
{
    public static readonly Dictionary<string, string[]> RolePermissionMappings = new()
    {
        [Roles.Admin] = new[]
        {
            // Full system access
            Permissions.UsersRead, Permissions.UsersCreate, Permissions.UsersUpdate, Permissions.UsersDelete, Permissions.UsersManage,
            Permissions.RolesRead, Permissions.RolesCreate, Permissions.RolesUpdate, Permissions.RolesDelete, Permissions.RolesAssign,
            Permissions.PermissionsRead, Permissions.PermissionsAssign,
            Permissions.PartsRead, Permissions.PartsCreate, Permissions.PartsUpdate, Permissions.PartsDelete, Permissions.PartsApprove, Permissions.PartsManage,
            Permissions.InventoryRead, Permissions.InventoryUpdate, Permissions.InventoryManage,
            Permissions.OrdersRead, Permissions.OrdersCreate, Permissions.OrdersUpdate, Permissions.OrdersDelete, Permissions.OrdersProcess, Permissions.OrdersManage, Permissions.OrdersAssign,
            Permissions.MarketingRead, Permissions.MarketingCreate, Permissions.MarketingUpdate, Permissions.MarketingDelete, Permissions.DiscountsManage, Permissions.FeaturedItemsManage, Permissions.CampaignsManage,
            Permissions.FinanceRead, Permissions.FinanceReports, Permissions.AccountsPayable, Permissions.AccountsReceivable, Permissions.PaymentsManage, Permissions.InvoicesManage, Permissions.RefundsManage,
            Permissions.SupportRead, Permissions.SupportCreate, Permissions.SupportUpdate, Permissions.SupportManage, Permissions.TicketsManage, Permissions.CustomerInteract, Permissions.ComplaintsHandle, Permissions.OrdersOnBehalf,
            Permissions.DeliveryRead, Permissions.DeliveryUpdate, Permissions.DeliveryAssign, Permissions.DeliveryManage, Permissions.LogisticsManage, Permissions.DeliveryTracking, Permissions.RouteOptimization,
            Permissions.VendorsRead, Permissions.VendorsManage, Permissions.VendorOnboarding, Permissions.VendorPayments,
            Permissions.AffiliateRead, Permissions.AffiliateManage, Permissions.CommissionsManage,
            Permissions.ReportsRead, Permissions.ReportsExport, Permissions.AnalyticsRead, Permissions.DashboardAccess,
            Permissions.SystemRead, Permissions.SystemUpdate, Permissions.SystemManage, Permissions.AuditLogsRead
        },

        [Roles.Customer] = new[]
        {
            // Basic customer permissions
            Permissions.PartsRead,
            Permissions.OrdersRead, Permissions.OrdersCreate,
            Permissions.SupportCreate,
            Permissions.DashboardAccess
        },

        [Roles.Mechanic] = new[]
        {
            // Customer permissions + affiliate features
            Permissions.PartsRead,
            Permissions.OrdersRead, Permissions.OrdersCreate,
            Permissions.SupportCreate,
            Permissions.AffiliateRead,
            Permissions.CommissionsManage,
            Permissions.DashboardAccess,
            Permissions.ReportsRead
        },

        [Roles.Vendor] = new[]
        {
            // Vendor-specific permissions
            Permissions.PartsRead, Permissions.PartsCreate, Permissions.PartsUpdate,
            Permissions.InventoryRead, Permissions.InventoryUpdate,
            Permissions.OrdersRead, Permissions.OrdersUpdate,
            Permissions.VendorsRead,
            Permissions.DashboardAccess,
            Permissions.ReportsRead,
            Permissions.SupportCreate
        },

        [Roles.Marketer] = new[]
        {
            // Marketing and promotions management
            Permissions.PartsRead, Permissions.PartsUpdate,
            Permissions.MarketingRead, Permissions.MarketingCreate, Permissions.MarketingUpdate, Permissions.MarketingDelete,
            Permissions.DiscountsManage, Permissions.FeaturedItemsManage, Permissions.CampaignsManage,
            Permissions.AnalyticsRead, Permissions.DashboardAccess,
            Permissions.ReportsRead, Permissions.ReportsExport,
            Permissions.SupportRead
        },

        [Roles.Finance] = new[]
        {
            // Finance and accounting operations
            Permissions.FinanceRead, Permissions.FinanceReports,
            Permissions.AccountsPayable, Permissions.AccountsReceivable,
            Permissions.PaymentsManage, Permissions.InvoicesManage, Permissions.RefundsManage,
            Permissions.OrdersRead, Permissions.OrdersUpdate,
            Permissions.VendorPayments,
            Permissions.CommissionsManage,
            Permissions.ReportsRead, Permissions.ReportsExport,
            Permissions.AnalyticsRead, Permissions.DashboardAccess,
            Permissions.SupportRead
        },

        [Roles.CustomerSupport] = new[]
        {
            // Customer support and assistance
            Permissions.SupportRead, Permissions.SupportCreate, Permissions.SupportUpdate, Permissions.SupportManage,
            Permissions.TicketsManage, Permissions.CustomerInteract, Permissions.ComplaintsHandle,
            Permissions.OrdersRead, Permissions.OrdersCreate, Permissions.OrdersUpdate, Permissions.OrdersOnBehalf,
            Permissions.PartsRead,
            Permissions.UsersRead, Permissions.UsersUpdate,
            Permissions.RefundsManage,
            Permissions.DashboardAccess,
            Permissions.ReportsRead
        },

        [Roles.Delivery] = new[]
        {
            // Delivery operations
            Permissions.DeliveryRead, Permissions.DeliveryUpdate,
            Permissions.DeliveryTracking,
            Permissions.OrdersRead, Permissions.OrdersUpdate,
            Permissions.DashboardAccess,
            Permissions.SupportCreate
        },

        [Roles.DeliveryManager] = new[]
        {
            // Delivery and logistics management
            Permissions.DeliveryRead, Permissions.DeliveryUpdate, Permissions.DeliveryAssign, Permissions.DeliveryManage,
            Permissions.LogisticsManage, Permissions.DeliveryTracking, Permissions.RouteOptimization,
            Permissions.OrdersRead, Permissions.OrdersUpdate, Permissions.OrdersAssign,
            Permissions.UsersRead, Permissions.UsersUpdate,
            Permissions.ReportsRead, Permissions.ReportsExport,
            Permissions.AnalyticsRead, Permissions.DashboardAccess,
            Permissions.SupportRead, Permissions.SupportCreate
        }
    };

    public static string[] GetPermissionsForRole(string roleName)
    {
        return RolePermissionMappings.TryGetValue(roleName, out var permissions) 
            ? permissions 
            : Array.Empty<string>();
    }

    public static bool RoleHasPermission(string roleName, string permission)
    {
        var rolePermissions = GetPermissionsForRole(roleName);
        return rolePermissions.Contains(permission);
    }
}
