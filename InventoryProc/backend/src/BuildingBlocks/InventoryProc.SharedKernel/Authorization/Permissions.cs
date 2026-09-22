namespace InventoryProc.SharedKernel.Authorization;

/// <summary>
/// Defines all permissions in the system
/// </summary>
public static class Permissions
{
    // Product Permissions
    public const string ProductsView = "Permissions.Products.View";
    public const string ProductsCreate = "Permissions.Products.Create";
    public const string ProductsEdit = "Permissions.Products.Edit";
    public const string ProductsDelete = "Permissions.Products.Delete";

    // Category Permissions
    public const string CategoriesView = "Permissions.Categories.View";
    public const string CategoriesManage = "Permissions.Categories.Manage";

    // Brand Permissions
    public const string BrandsView = "Permissions.Brands.View";
    public const string BrandsManage = "Permissions.Brands.Manage";

    // Customer Permissions
    public const string CustomersView = "Permissions.Customers.View";
    public const string CustomersCreate = "Permissions.Customers.Create";
    public const string CustomersEdit = "Permissions.Customers.Edit";
    public const string CustomersDelete = "Permissions.Customers.Delete";

    // Sales Order Permissions
    public const string SalesOrdersView = "Permissions.SalesOrders.View";
    public const string SalesOrdersCreate = "Permissions.SalesOrders.Create";
    public const string SalesOrdersEdit = "Permissions.SalesOrders.Edit";
    public const string SalesOrdersConfirm = "Permissions.SalesOrders.Confirm";
    public const string SalesOrdersCancel = "Permissions.SalesOrders.Cancel";
    public const string SalesOrdersDelete = "Permissions.SalesOrders.Delete";

    // Invoice Permissions
    public const string InvoicesView = "Permissions.Invoices.View";
    public const string InvoicesCreate = "Permissions.Invoices.Create";
    public const string InvoicesEdit = "Permissions.Invoices.Edit";
    public const string InvoicesRecordPayment = "Permissions.Invoices.RecordPayment";

    // Vendor Permissions
    public const string VendorsView = "Permissions.Vendors.View";
    public const string VendorsCreate = "Permissions.Vendors.Create";
    public const string VendorsEdit = "Permissions.Vendors.Edit";
    public const string VendorsDelete = "Permissions.Vendors.Delete";

    // Purchase Order Permissions
    public const string PurchaseOrdersView = "Permissions.PurchaseOrders.View";
    public const string PurchaseOrdersCreate = "Permissions.PurchaseOrders.Create";
    public const string PurchaseOrdersEdit = "Permissions.PurchaseOrders.Edit";
    public const string PurchaseOrdersApprove = "Permissions.PurchaseOrders.Approve";
    public const string PurchaseOrdersCancel = "Permissions.PurchaseOrders.Cancel";
    public const string PurchaseOrdersDelete = "Permissions.PurchaseOrders.Delete";

    // GRN Permissions
    public const string GRNView = "Permissions.GRN.View";
    public const string GRNCreate = "Permissions.GRN.Create";

    // Report Permissions
    public const string ReportsView = "Permissions.Reports.View";
    public const string ReportsExport = "Permissions.Reports.Export";

    // User Management Permissions
    public const string UsersView = "Permissions.Users.View";
    public const string UsersCreate = "Permissions.Users.Create";
    public const string UsersEdit = "Permissions.Users.Edit";
    public const string UsersDelete = "Permissions.Users.Delete";
    public const string UsersManageRoles = "Permissions.Users.ManageRoles";

    // Activity Log Permissions
    public const string ActivityLogsView = "Permissions.ActivityLogs.View";
}

/// <summary>
/// Maps roles to their permissions
/// </summary>
public static class RolePermissions
{
    public static Dictionary<string, string[]> GetRolePermissions()
    {
        return new Dictionary<string, string[]>
        {
            ["Admin"] = new[]
            {
                // Admin has all permissions
                Permissions.ProductsView, Permissions.ProductsCreate, Permissions.ProductsEdit, Permissions.ProductsDelete,
                Permissions.CategoriesView, Permissions.CategoriesManage,
                Permissions.BrandsView, Permissions.BrandsManage,
                Permissions.CustomersView, Permissions.CustomersCreate, Permissions.CustomersEdit, Permissions.CustomersDelete,
                Permissions.SalesOrdersView, Permissions.SalesOrdersCreate, Permissions.SalesOrdersEdit,
                Permissions.SalesOrdersConfirm, Permissions.SalesOrdersCancel, Permissions.SalesOrdersDelete,
                Permissions.InvoicesView, Permissions.InvoicesCreate, Permissions.InvoicesEdit, Permissions.InvoicesRecordPayment,
                Permissions.VendorsView, Permissions.VendorsCreate, Permissions.VendorsEdit, Permissions.VendorsDelete,
                Permissions.PurchaseOrdersView, Permissions.PurchaseOrdersCreate, Permissions.PurchaseOrdersEdit,
                Permissions.PurchaseOrdersApprove, Permissions.PurchaseOrdersCancel, Permissions.PurchaseOrdersDelete,
                Permissions.GRNView, Permissions.GRNCreate,
                Permissions.ReportsView, Permissions.ReportsExport,
                Permissions.UsersView, Permissions.UsersCreate, Permissions.UsersEdit, Permissions.UsersDelete, Permissions.UsersManageRoles,
                Permissions.ActivityLogsView
            },
            ["Manager"] = new[]
            {
                // Manager can do most operations but not delete users or manage certain critical areas
                Permissions.ProductsView, Permissions.ProductsCreate, Permissions.ProductsEdit,
                Permissions.CategoriesView, Permissions.CategoriesManage,
                Permissions.BrandsView, Permissions.BrandsManage,
                Permissions.CustomersView, Permissions.CustomersCreate, Permissions.CustomersEdit,
                Permissions.SalesOrdersView, Permissions.SalesOrdersCreate, Permissions.SalesOrdersEdit,
                Permissions.SalesOrdersConfirm, Permissions.SalesOrdersCancel,
                Permissions.InvoicesView, Permissions.InvoicesCreate, Permissions.InvoicesEdit, Permissions.InvoicesRecordPayment,
                Permissions.VendorsView, Permissions.VendorsCreate, Permissions.VendorsEdit,
                Permissions.PurchaseOrdersView, Permissions.PurchaseOrdersCreate, Permissions.PurchaseOrdersEdit,
                Permissions.PurchaseOrdersApprove, Permissions.PurchaseOrdersCancel,
                Permissions.GRNView, Permissions.GRNCreate,
                Permissions.ReportsView, Permissions.ReportsExport,
                Permissions.UsersView,
                Permissions.ActivityLogsView
            },
            ["SalesStaff"] = new[]
            {
                // Sales staff can manage products, customers, and sales
                Permissions.ProductsView, Permissions.ProductsCreate, Permissions.ProductsEdit,
                Permissions.CategoriesView,
                Permissions.BrandsView,
                Permissions.CustomersView, Permissions.CustomersCreate, Permissions.CustomersEdit,
                Permissions.SalesOrdersView, Permissions.SalesOrdersCreate, Permissions.SalesOrdersEdit,
                Permissions.InvoicesView, Permissions.InvoicesRecordPayment,
                Permissions.VendorsView,
                Permissions.PurchaseOrdersView,
                Permissions.GRNView,
                Permissions.ReportsView
            },
            ["Viewer"] = new[]
            {
                // Viewer can only view data, no modifications
                Permissions.ProductsView,
                Permissions.CategoriesView,
                Permissions.BrandsView,
                Permissions.CustomersView,
                Permissions.SalesOrdersView,
                Permissions.InvoicesView,
                Permissions.VendorsView,
                Permissions.PurchaseOrdersView,
                Permissions.GRNView,
                Permissions.ReportsView
            }
        };
    }
}
