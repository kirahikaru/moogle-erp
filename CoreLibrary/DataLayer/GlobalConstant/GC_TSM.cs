namespace DataLayer.GlobalConstant;

public static class CPUFormFactors
{
	public const string LAPTOP = "LAPTOP";
	public const string DESKTOP = "DESKTOP";
	public const string LAPTOP_DESKTOP = "LAPTOP-DESKTOP";

	public static string GetDisplayText(string? formFactor)
	{
		return formFactor switch
		{
			LAPTOP => "Laptop",
			DESKTOP => "Desktop",
			LAPTOP_DESKTOP => "Laptop, Desktop",
			_ => "",
		};

	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
			[
				new DropdownSelectItem { Key = LAPTOP, Value = GetDisplayText(LAPTOP) },
				new DropdownSelectItem { Key = DESKTOP, Value = GetDisplayText(DESKTOP) },
				new DropdownSelectItem { Key = LAPTOP_DESKTOP, Value = GetDisplayText(LAPTOP_DESKTOP) }
			];

		return list;
	}
}

public static class RAMSizes
{
    public static List<DropdownSelectItem> GetForDropdown()
    {
        List<DropdownSelectItem> list =
            [
                new DropdownSelectItem { Id = 8, Value = "8 GB" },
				new DropdownSelectItem { Id = 16, Value = "16 GB" },
				new DropdownSelectItem { Id = 32, Value = "32 GB" },
				new DropdownSelectItem { Id = 64, Value = "64 GB" },
				new DropdownSelectItem { Id = 96, Value = "96 GB" },
				new DropdownSelectItem { Id = 128, Value = "128 GB" },
				new DropdownSelectItem { Id = 256, Value = "256 GB" },
			];

        return list;
    }
}

public static class RAMTypes
{
    public const string DEVICE_LAPTOP = "LAPTOP";
	public const string DEVICE_PC = "PC";
	public const string DEVICE_DESKTOP = "DESKTOP";
	public const string DEVICE_SMART_PHONE = "SMART-PHONE";
	public static List<DropdownSelectItem> GetForDropdown(string deviceType = DEVICE_PC)
    {
        switch (deviceType)
        {
            case DEVICE_LAPTOP:
                return [
					        new DropdownSelectItem { Key = "LPDDR5", Value = "LPDDR5/5X" },
							new DropdownSelectItem { Key = "LPDDR4/4X", Value = "LPDDR4/4X" },
							new DropdownSelectItem { Key = "DDR5", Value = "DDR5 SO-DIMM" },
							new DropdownSelectItem { Key = "DDR4", Value = "DDR4 SO-DIMM" },
							new DropdownSelectItem { Key = "DDR3", Value = "DDR3 SO-DIMM" },
					   ];
			case DEVICE_PC:
            case DEVICE_DESKTOP:
            case DEVICE_SMART_PHONE:
				return [
							new DropdownSelectItem { Key = "LPDDR5", Value = "LPDDR5/5X" },
							new DropdownSelectItem { Key = "LPDDR4/4X", Value = "LPDDR4/4X" },
							new DropdownSelectItem { Key = "DDR5", Value = "DDR5 SO-DIMM" },
							new DropdownSelectItem { Key = "DDR4", Value = "DDR4 SO-DIMM" },
							new DropdownSelectItem { Key = "DDR3", Value = "DDR3 SO-DIMM" },
					   ];
			default:
				return [];
		}
    }
}

public static class LaptopStorageTypes
{
	public const string SSD_M2_NVME_PCIE4 = "SSD-M.2_NVMe_PCIe_4.0";

	public static string GetDisplayText(string? storageType)
	{
		switch (storageType)
		{
			case SSD_M2_NVME_PCIE4:
				return "M.2 NVMe™ PCIe® 4.0 SSD";
			default:
				return "";
		}
	}

	public static List<DropdownSelectItem> GetForDropdown()
	{
		List<DropdownSelectItem> list =
			[
				new DropdownSelectItem { Key = SSD_M2_NVME_PCIE4, Value = GetDisplayText(SSD_M2_NVME_PCIE4) },
			];

		return list;
	}
}

public static class StorageDriveTypes
{
	public const string HDD = "HDD";
	public const string SSD = "SSD";
}
