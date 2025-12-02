namespace DataLayer.GlobalConstant.Pru;

public static class PruLBUs
{
	public const string PCLA = "PCLA";
	public const string PLAL = "PLAL";
	public const string PMLI = "PMLI";

	public static string GetDisplayText(string? maritalStatus)
	{
		return maritalStatus switch
		{
			PCLA => "PCLA (Cambodia)",
			PLAL => "PLAL (Laos)",
			PMLI => "PMLI (Myanmar)",
			_ => ""
		};
	}

	public static Dictionary<string, string> GetAll()
	{
		Dictionary<string, string> list = new()
		{
			{ PCLA, GetDisplayText(PCLA) },
			{ PLAL, GetDisplayText(PLAL) },
			{ PMLI, GetDisplayText(PMLI) },
		};

		return list;
	}

	public static Dictionary<string, string> GetFlagIconPaths()
	{
		Dictionary<string, string> list = new()
		{
			{ PCLA, "/image/flag/flag-kh-cambodia.svg" },
			{ PLAL, "/image/flag/flag-la-laos.svg" },
			{ PMLI, "/image/flag/flag-mm-myanmar.svg" },
		};

		return list;
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		return [
			new DropdownSelectItem { Key = PCLA, Value = GetDisplayText(PCLA) },
			new DropdownSelectItem { Key = PLAL, Value = GetDisplayText(PLAL) },
			new DropdownSelectItem { Key = PMLI, Value = GetDisplayText(PMLI) },
		];
	}
}

public static class AssetTypes
{
    public const string Hardware = "Hardware";
    public const string Software = "Software";
    public const string CloudInfra = "Cloud";

    public static string GetDisplayText(string? assetType)
    {
        return assetType switch
        {
            Hardware => "Hardware",
            Software => "Software",
            CloudInfra => "Cloud Infrastructure",
            _ => ""
        };
    }

    public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = Hardware, Value = Hardware },
			new DropdownSelectItem { Key = Software, Value = Software },
			new DropdownSelectItem { Key = CloudInfra, Value = CloudInfra }
			];
	}

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { Hardware, GetDisplayText(Hardware) },
            { Software, GetDisplayText(Software) },
            { CloudInfra, GetDisplayText(CloudInfra) },
        };

        return list;
    }
}

public static class VendorStatuses
{
    public const string ACTIVE = "ACTIVE";
    public const string TERMINATED = "TERMINATED";
    public const string ONBOARDING = "ONBOARDING";
    public const string INACTIVE = "INACTIVE";

	public static string GetDisplayText(string? assetType)
	{
		return assetType switch
		{
			ACTIVE => "Active",
			TERMINATED => "Terminated",
			ONBOARDING => "Onboarding",
            INACTIVE => "Inactive",
			_ => ""
		};
	}

	public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = ACTIVE, Value = GetDisplayText(ACTIVE) },
			new DropdownSelectItem { Key = TERMINATED, Value = GetDisplayText(TERMINATED) },
			new DropdownSelectItem { Key = ONBOARDING, Value = GetDisplayText(ONBOARDING) },
			new DropdownSelectItem { Key = INACTIVE, Value = GetDisplayText(INACTIVE) },
			];
	}
}

public static class AssetStates
{
    public const string BRAND_NEW = "BRAND-NEW";

	/// <summary>
	/// Used (Good Condition)
	/// </summary>
    public const string USED_GOOD = "USED-GOOD";

	/// <summary>
	/// Used (Poor Condition)
	/// </summary>
	public const string USED_POOR = "USED-POOR";
	/// <summary>
	/// Used (Esclated Security Risk)
	/// </summary>
	public const string USED_ESR = "USED-ESR";

	/// <summary>
	/// End-Of-Life
	/// </summary>
	public const string EOL = "EOL";
	/// <summary>
	/// End-Of-Support
	/// </summary>
	public const string EOS = "EOS";
    public const string BROKEN_UNREPAIRABLE = "BROKEN-UNREPAIRABLE";
	public const string NA = "N/A";
	public const string OFFBOARDED = "OFFBOARDED";

	public static string GetDisplayText(string? assetState)
	{
		return assetState switch
		{
			BRAND_NEW => "Brand New",
			USED_GOOD => "Used (Good Cond.)",
			EOL => "End-Of-Life",
			EOS => "End-Of-Support",
			BROKEN_UNREPAIRABLE => "Broken Unrepairable",
			USED_POOR => "Used (Poor Perf.)",
			USED_ESR => "Used (Elevated Security Risk)",
			OFFBOARDED => "Offboarded",
			NA => "N/A",
			_ => ""
		};
	}

	public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = BRAND_NEW, Value = GetDisplayText(BRAND_NEW) },
			new DropdownSelectItem { Key = USED_GOOD, Value = GetDisplayText(USED_GOOD) },
			new DropdownSelectItem { Key = EOL, Value = GetDisplayText(EOL) },
			new DropdownSelectItem { Key = EOS, Value = GetDisplayText(EOS) },
			new DropdownSelectItem { Key = BROKEN_UNREPAIRABLE, Value = GetDisplayText(BROKEN_UNREPAIRABLE) },
			new DropdownSelectItem { Key = USED_POOR, Value = GetDisplayText(USED_POOR) },
			new DropdownSelectItem { Key = USED_ESR, Value = GetDisplayText(USED_ESR) },
			new DropdownSelectItem { Key = OFFBOARDED, Value = GetDisplayText(OFFBOARDED) },
			new DropdownSelectItem { Key = NA, Value = GetDisplayText(NA) },
			];
	}
}

public static class AssetLifeCycleStatuses
{
	public const string DRAFT = "DRAFT";
	public const string IN_USE = "IN-USE";
	public const string IN_STOCK = "IN-STOCK";
	public const string PENDING_REVIEW = "PENDING-REVIEW";
	public const string PENDING_WRITE_OFF = "PENDING-WRITE-OFF";
	public const string TERMINATED = "TERMINATED";
	public const string PENDING_TERMINATION = "PENDING-TERMINATION";
	public const string WRITTEN_OFF = "WRITTEN-OFF";
	public const string DISPOSED = "DISPOSED";

	public static string GetDisplayText(string? s)
	{
		return s switch
		{
			DRAFT => "Draft",
			IN_USE => "In-Use",
			IN_STOCK => "In-Stock",
			PENDING_REVIEW => "Pending Review",
			PENDING_WRITE_OFF => "Pending Write-Off",
			PENDING_TERMINATION => "Pending Termination",
			TERMINATED => "Terminated",
			WRITTEN_OFF => "Written-Off",
			DISPOSED => "Disposed",
			_ => ""
		};
	}

	public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = DRAFT, Value = GetDisplayText(DRAFT) },
			new DropdownSelectItem { Key = IN_STOCK, Value = GetDisplayText(IN_STOCK) },
			new DropdownSelectItem { Key = IN_USE, Value = GetDisplayText(IN_USE) },
			new DropdownSelectItem { Key = PENDING_REVIEW, Value = GetDisplayText(PENDING_REVIEW) },
			new DropdownSelectItem { Key = PENDING_WRITE_OFF, Value = GetDisplayText(PENDING_WRITE_OFF) },
			new DropdownSelectItem { Key = PENDING_TERMINATION, Value = GetDisplayText(PENDING_TERMINATION) },
			new DropdownSelectItem { Key = TERMINATED, Value = GetDisplayText(TERMINATED) },
			new DropdownSelectItem { Key = WRITTEN_OFF, Value = GetDisplayText(WRITTEN_OFF) },
			new DropdownSelectItem { Key = DISPOSED, Value = GetDisplayText(DISPOSED) },
			];
	}
}

public static class QuotationWFStatuses
{
	public const string CANCELLED = "CANCELLED";
	public const string UNDER_REVIEW = "UNDER-REVIEW";
	public const string CONFIRMED = "CONFIRMED";
	public const string PO_RAISED = "PO_RAISED";
	public const string INVOICED = "INVOICED";
	public const string COMPLETE = "COMPLETE";

	public static string GetDisplayText(string? wfs)
	{
		return wfs switch
		{
			CANCELLED => "Cancelled",
			UNDER_REVIEW => "Under Review",
			CONFIRMED => "Confirmed",
			PO_RAISED => "PO Raised",
			INVOICED => "Invoiced",
			COMPLETE => "Complete",
			_ => ""
		};
	}

	public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = CANCELLED, Value = GetDisplayText(CANCELLED) },
			new DropdownSelectItem { Key = UNDER_REVIEW, Value = GetDisplayText(UNDER_REVIEW) },
			new DropdownSelectItem { Key = CONFIRMED, Value = GetDisplayText(CONFIRMED) },
			new DropdownSelectItem { Key = PO_RAISED, Value = GetDisplayText(PO_RAISED) },
			new DropdownSelectItem { Key = INVOICED, Value = GetDisplayText(INVOICED) },
			new DropdownSelectItem { Key = COMPLETE, Value = GetDisplayText(COMPLETE) },
			];
	}
}

/// <summary>
/// Purchase Order Workflow Statuses
/// </summary>
public static class PurchaseOrderWFStatuses
{
	public const string CANCELLED = "CANCELLED";
	public const string PR_RAISED = "PR_RAISED";
	public const string PR_REJECTED = "PR_REJECTED";
	public const string PR_APPROVED = "PR_APPROVED";
	public const string PREPARING_PO = "PREPARING_PO";
	public const string COMPLETE = "COMPLETE";

	public static string GetDisplayText(string? wfs)
	{
		return wfs switch
		{
			CANCELLED => "Cancelled",
			PR_RAISED => "PR Raised",
			PR_REJECTED => "PR Rejected",
			PR_APPROVED => "PR Approved",
			PREPARING_PO => "Preparing PO",
			COMPLETE => "Complete",
			_ => ""
		};
	}

	public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = CANCELLED, Value = GetDisplayText(CANCELLED) },
			new DropdownSelectItem { Key = PR_RAISED, Value = GetDisplayText(PR_RAISED) },
			new DropdownSelectItem { Key = PR_REJECTED, Value = GetDisplayText(PR_REJECTED) },
			new DropdownSelectItem { Key = PR_APPROVED, Value = GetDisplayText(PR_APPROVED) },
			new DropdownSelectItem { Key = PREPARING_PO, Value = GetDisplayText(PREPARING_PO) },
			new DropdownSelectItem { Key = COMPLETE, Value = GetDisplayText(COMPLETE) },
			];
	}
}

/// <summary>
/// Purchase Order Workflow Statuses
/// </summary>
public static class InvoiceWFStatuses
{
	public const string CANCELLED = "CANCELLED";
	public const string REGISTERED = "REGISTERED";
	/// <summary>
	/// Submitted to Accounting System
	/// </summary>
	public const string SUBMITTED = "SUBMITTED";
	/// <summary>
	/// Approved in accounting system
	/// </summary>
	public const string APPROVED = "APPROVED";
	public const string REJECTED = "REJECTED";
	public const string COMPLETE = "COMPLETE";

	public static string GetDisplayText(string? wfs)
	{
		return wfs switch
		{
			CANCELLED => "Cancelled",
			REGISTERED => "Registered",
			SUBMITTED => "Submitted",
			APPROVED => "Approved",
			REJECTED => "Rejected",
			COMPLETE => "Complete",
			_ => ""
		};
	}

	public static IEnumerable<DropdownSelectItem> GetForDropdownList()
	{
		return [
			new DropdownSelectItem { Key = CANCELLED, Value = GetDisplayText(CANCELLED) },
			new DropdownSelectItem { Key = REGISTERED, Value = GetDisplayText(REGISTERED) },
			new DropdownSelectItem { Key = SUBMITTED, Value = GetDisplayText(SUBMITTED) },
			new DropdownSelectItem { Key = REJECTED, Value = GetDisplayText(REJECTED) },
			new DropdownSelectItem { Key = APPROVED, Value = GetDisplayText(APPROVED) },
			new DropdownSelectItem { Key = COMPLETE, Value = GetDisplayText(COMPLETE) },
			];
	}
}

public static class PruDepartments
{
	public const string Actuary = "Actuary";
	public const string Finance = "Finance";
	public const string Marketing = "Marketing";
	public const string TechOps = "Technology & Operation";
	public const string OpsTech_Tech = "Technology & Operation | Technology";
	public const string OpsTech_Ops = "Technology & Operation | Operation";
	public const string LegalRiskCompliance = "Legal, Risk & Compliance";
	public const string RiskCompliance = "Risk & Compliance";
	public const string LegalGR = "Legal & Goverment Relation";
	public const string HumanResource = "Human Resource";
	public const string SaleDist = "Sale & Distribution";
	public const string CEOOffice = "CEO Office";

	/// <summary>
	/// Obsolete department
	/// It has been merged into Operation and become Technology & Operation Department on 1-Nov-2025
	/// </summary>
	public const string TED = "Transformation & Effeciency";

	public static IEnumerable<string> GetAll()
	{
		return [
			Actuary, Finance, Marketing, TechOps, OpsTech_Ops, OpsTech_Tech, LegalRiskCompliance, RiskCompliance, LegalGR, HumanResource, SaleDist, CEOOffice
			];
	}

	public static IEnumerable<string> GetLatest()
	{
		return [
			Actuary, Finance, Marketing, TechOps, OpsTech_Ops, OpsTech_Tech, LegalRiskCompliance, RiskCompliance, LegalGR, HumanResource, SaleDist, CEOOffice
			];
	}
}