namespace Identity.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Mechanic = "Mechanic";
    public const string Vendor = "Vendor";
    public const string Marketer = "Marketer";
    public const string Finance = "Finance";
    public const string CustomerSupport = "CustomerSupport";
    public const string Delivery = "Delivery";
    public const string DeliveryManager = "DeliveryManager";
    
    public static readonly string[] AllRoles = 
    {
        Admin,
        Customer,
        Mechanic,
        Vendor,
        Marketer,
        Finance,
        CustomerSupport,
        Delivery,
        DeliveryManager
    };
}

public static class Permissions
{
    // User Management
    public const string UsersRead = "users.read";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";
    public const string UsersManage = "users.manage";
    
    // Role Management
    public const string RolesRead = "roles.read";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";
    public const string RolesDelete = "roles.delete";
    public const string RolesAssign = "roles.assign";
    
    // Permission Management
    public const string PermissionsRead = "permissions.read";
    public const string PermissionsAssign = "permissions.assign";
    
    // Product/Parts Management
    public const string PartsRead = "parts.read";
    public const string PartsCreate = "parts.create";
    public const string PartsUpdate = "parts.update";
    public const string PartsDelete = "parts.delete";
    public const string PartsApprove = "parts.approve";
    public const string PartsManage = "parts.manage";
    
    // Inventory Management
    public const string InventoryRead = "inventory.read";
    public const string InventoryUpdate = "inventory.update";
    public const string InventoryManage = "inventory.manage";
    
    // Order Management
    public const string OrdersRead = "orders.read";
    public const string OrdersCreate = "orders.create";
    public const string OrdersUpdate = "orders.update";
    public const string OrdersDelete = "orders.delete";
    public const string OrdersProcess = "orders.process";
    public const string OrdersManage = "orders.manage";
    public const string OrdersAssign = "orders.assign";
    
    // Marketing & Promotions
    public const string MarketingRead = "marketing.read";
    public const string MarketingCreate = "marketing.create";
    public const string MarketingUpdate = "marketing.update";
    public const string MarketingDelete = "marketing.delete";
    public const string DiscountsManage = "discounts.manage";
    public const string FeaturedItemsManage = "featured-items.manage";
    public const string CampaignsManage = "campaigns.manage";
    
    // Finance & Accounting
    public const string FinanceRead = "finance.read";
    public const string FinanceReports = "finance.reports";
    public const string AccountsPayable = "accounts.payable";
    public const string AccountsReceivable = "accounts.receivable";
    public const string PaymentsManage = "payments.manage";
    public const string InvoicesManage = "invoices.manage";
    public const string RefundsManage = "refunds.manage";
    
    // Customer Support
    public const string SupportRead = "support.read";
    public const string SupportCreate = "support.create";
    public const string SupportUpdate = "support.update";
    public const string SupportManage = "support.manage";
    public const string TicketsManage = "tickets.manage";
    public const string CustomerInteract = "customer.interact";
    public const string ComplaintsHandle = "complaints.handle";
    public const string OrdersOnBehalf = "orders.on-behalf";
    
    // Delivery & Logistics
    public const string DeliveryRead = "delivery.read";
    public const string DeliveryUpdate = "delivery.update";
    public const string DeliveryAssign = "delivery.assign";
    public const string DeliveryManage = "delivery.manage";
    public const string LogisticsManage = "logistics.manage";
    public const string DeliveryTracking = "delivery.tracking";
    public const string RouteOptimization = "route.optimization";
    
    // Vendor Management
    public const string VendorsRead = "vendors.read";
    public const string VendorsManage = "vendors.manage";
    public const string VendorOnboarding = "vendor.onboarding";
    public const string VendorPayments = "vendor.payments";
    
    // Affiliate/Mechanic Management
    public const string AffiliateRead = "affiliate.read";
    public const string AffiliateManage = "affiliate.manage";
    public const string CommissionsManage = "commissions.manage";
    
    // Reporting
    public const string ReportsRead = "reports.read";
    public const string ReportsExport = "reports.export";
    public const string AnalyticsRead = "analytics.read";
    public const string DashboardAccess = "dashboard.access";
    
    // System Administration
    public const string SystemRead = "system.read";
    public const string SystemUpdate = "system.update";
    public const string SystemManage = "system.manage";
    public const string AuditLogsRead = "audit-logs.read";
    
    public static readonly string[] AllPermissions = 
    {
        // User Management
        UsersRead, UsersCreate, UsersUpdate, UsersDelete, UsersManage,
        
        // Role Management
        RolesRead, RolesCreate, RolesUpdate, RolesDelete, RolesAssign,
        
        // Permission Management
        PermissionsRead, PermissionsAssign,
        
        // Parts Management
        PartsRead, PartsCreate, PartsUpdate, PartsDelete, PartsApprove, PartsManage,
        
        // Inventory Management
        InventoryRead, InventoryUpdate, InventoryManage,
        
        // Order Management
        OrdersRead, OrdersCreate, OrdersUpdate, OrdersDelete, OrdersProcess, OrdersManage, OrdersAssign,
        
        // Marketing & Promotions
        MarketingRead, MarketingCreate, MarketingUpdate, MarketingDelete, 
        DiscountsManage, FeaturedItemsManage, CampaignsManage,
        
        // Finance & Accounting
        FinanceRead, FinanceReports, AccountsPayable, AccountsReceivable, 
        PaymentsManage, InvoicesManage, RefundsManage,
        
        // Customer Support
        SupportRead, SupportCreate, SupportUpdate, SupportManage, 
        TicketsManage, CustomerInteract, ComplaintsHandle, OrdersOnBehalf,
        
        // Delivery & Logistics
        DeliveryRead, DeliveryUpdate, DeliveryAssign, DeliveryManage, 
        LogisticsManage, DeliveryTracking, RouteOptimization,
        
        // Vendor Management
        VendorsRead, VendorsManage, VendorOnboarding, VendorPayments,
        
        // Affiliate/Mechanic Management
        AffiliateRead, AffiliateManage, CommissionsManage,
        
        // Reporting
        ReportsRead, ReportsExport, AnalyticsRead, DashboardAccess,
        
        // System Administration
        SystemRead, SystemUpdate, SystemManage, AuditLogsRead
    };
}

public static class PermissionCategories
{
    public const string UserManagement = "User Management";
    public const string RoleManagement = "Role Management";
    public const string PermissionManagement = "Permission Management";
    public const string PartsManagement = "Parts Management";
    public const string InventoryManagement = "Inventory Management";
    public const string OrderManagement = "Order Management";
    public const string MarketingPromotions = "Marketing & Promotions";
    public const string FinanceAccounting = "Finance & Accounting";
    public const string CustomerSupport = "Customer Support";
    public const string DeliveryLogistics = "Delivery & Logistics";
    public const string VendorManagement = "Vendor Management";
    public const string AffiliateManagement = "Affiliate Management";
    public const string Reporting = "Reporting";
    public const string SystemAdministration = "System Administration";
}

public static class TwoFactorTypes
{
    public const string Email = "Email";
    public const string SMS = "SMS";
    public const string GoogleAuth = "GoogleAuth";
}

public static class DeviceTypes
{
    public const string Mobile = "Mobile";
    public const string Desktop = "Desktop";
    public const string Tablet = "Tablet";
    public const string Unknown = "Unknown";
}

public static class LoginResults
{
    public const string Success = "Success";
    public const string InvalidCredentials = "Invalid Credentials";
    public const string AccountLocked = "Account Locked";
    public const string AccountInactive = "Account Inactive";
    public const string TwoFactorRequired = "Two Factor Required";
    public const string EmailNotConfirmed = "Email Not Confirmed";
}
