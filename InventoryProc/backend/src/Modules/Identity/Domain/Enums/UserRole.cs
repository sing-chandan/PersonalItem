namespace InventoryProc.Modules.Identity.Domain.Enums;

public enum UserRole
{
    Admin = 1,      // Full access to everything
    Manager = 2,    // Can manage inventory, sales, purchases
    Staff = 3,      // Can create orders, manage inventory
    Viewer = 4      // Read-only access
}
