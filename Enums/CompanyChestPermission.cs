namespace LlamaLibrary.Enums
{
    /// <summary>
    /// Represents the access permission levels for a Free Company chest.
    /// </summary>
    public enum CompanyChestPermission
    {
        /// <summary>No access to the specified tab or resource.</summary>
        NoAccess = 0,
        /// <summary>Can view contents but cannot withdraw or deposit.</summary>
        ViewOnly = 1,
        /// <summary>Can only deposit items/gil; withdrawal is not permitted.</summary>
        DepositOnly = 2,
        /// <summary>Full access to both withdraw and deposit items/gil.</summary>
        FullAccess = 3
    }
}