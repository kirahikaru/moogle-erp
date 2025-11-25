using DataLayer.Models;

namespace DataLayer.GlobalConstant;

#region HIM - HOME INVENTORY MANAGEMENT
public static class BoardgameStateController
{
    public static Dictionary<string, string> GetNextValidActions(string? currentState)
    {
		return currentState switch
		{
			ObjectStates.NEW => new()
				{
					{ ObjectStateActions.SAVE_DRAFT, ObjectStateActions.GetDisplayText(ObjectStateActions.SAVE_DRAFT) },
					{ ObjectStateActions.PURCHASE, ObjectStateActions.GetDisplayText(ObjectStateActions.PURCHASE) },
					{ ObjectStateActions.ADD_WISHLIST, ObjectStateActions.GetDisplayText(ObjectStateActions.ADD_WISHLIST) },
				},
			ObjectStates.BNIB => new()
				{
					{ ObjectStateActions.START_USING, ObjectStateActions.GetDisplayText(ObjectStateActions.START_USING) },
					{ ObjectStateActions.GIVEN_OTHER, ObjectStateActions.GetDisplayText(ObjectStateActions.GIVEN_OTHER) },
					{ ObjectStateActions.LEND_OUT, ObjectStateActions.GetDisplayText(ObjectStateActions.LEND_OUT) },
				},
            ObjectStates.IN_USE => new()
				{
					{ ObjectStateActions.BROKE, ObjectStateActions.GetDisplayText(ObjectStateActions.BROKE) },
					{ ObjectStateActions.GIVEN_OTHER, ObjectStateActions.GetDisplayText(ObjectStateActions.GIVEN_OTHER) },
					{ ObjectStateActions.LEND_OUT, ObjectStateActions.GetDisplayText(ObjectStateActions.LEND_OUT) },
					{ ObjectStateActions.WRITE_OFF, ObjectStateActions.GetDisplayText(ObjectStateActions.WRITE_OFF) }
				},
            ObjectStates.LOANED => new()
            {
				{ ObjectStateActions.RETURN, ObjectStateActions.GetDisplayText(ObjectStateActions.RETURN) },
				{ ObjectStateActions.LOSE, ObjectStateActions.GetDisplayText(ObjectStateActions.LOSE) },
				{ ObjectStateActions.WRITE_OFF, ObjectStateActions.GetDisplayText(ObjectStateActions.WRITE_OFF) }
			},
			ObjectStates.WISHLIST => new()
			{
				{ ObjectStateActions.PURCHASE, ObjectStateActions.GetDisplayText(ObjectStateActions.PURCHASE) }
			},
			_ => new()
				{
					
				},
		};
	}

    public static string? GetResultingTargetState(string currentState, string action)
    {
        string cond = currentState + "|" + action;


		return cond switch
        {
			ObjectStates.NEW + "|" + ObjectStateActions.SAVE_DRAFT => ObjectStates.DRAFT,
			ObjectStates.NEW + "|" + ObjectStateActions.ADD_WISHLIST => ObjectStates.WISHLIST,
			ObjectStates.NEW + "|" + ObjectStateActions.PURCHASE => ObjectStates.BNIB,
            ObjectStates.DRAFT + "|" + ObjectStateActions.PURCHASE => ObjectStates.BNIB,
            ObjectStates.WISHLIST + "|" + ObjectStateActions.PURCHASE => ObjectStates.BNIB,
            ObjectStates.BNIB + "|" + ObjectStateActions.START_USING => ObjectStates.IN_USE,
			ObjectStates.IN_USE + "|" + ObjectStateActions.LEND_OUT => ObjectStates.LOANED,
			ObjectStates.IN_USE + "|" + ObjectStateActions.BROKE => ObjectStates.BROKEN,
            ObjectStates.IN_USE + "|" + ObjectStateActions.WRITE_OFF => ObjectStates.WRITTEN_OFF,
            _ => null
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new()
        {
            { ObjectStates.IN_USE, ObjectStates.GetDisplayText(ObjectStates.IN_USE) },
            { ObjectStates.WISHLIST, ObjectStates.GetDisplayText(ObjectStates.WISHLIST) },
            { ObjectStates.LOST, ObjectStates.GetDisplayText(ObjectStates.LOST) },
            { ObjectStates.LOANED, ObjectStates.GetDisplayText(ObjectStates.LOANED) }
        };
        return list;
    }

	public static List<DropdownSelectItem> GetForDropdownSelect()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem() { Key = ObjectStates.IN_USE, Value = ObjectStates.GetDisplayText(ObjectStates.IN_USE) },
			new DropdownSelectItem() { Key = ObjectStates.DRAFT, Value = ObjectStates.GetDisplayText(ObjectStates.DRAFT) },
			new DropdownSelectItem() { Key = ObjectStates.WISHLIST, Value = ObjectStates.GetDisplayText(ObjectStates.WISHLIST) },
			new DropdownSelectItem() { Key = ObjectStates.LOST, Value = ObjectStates.GetDisplayText(ObjectStates.LOST) },
			new DropdownSelectItem() { Key = ObjectStates.SOLD, Value = ObjectStates.GetDisplayText(ObjectStates.SOLD) },
			new DropdownSelectItem() { Key = ObjectStates.LOANED, Value = ObjectStates.GetDisplayText(ObjectStates.LOANED) },
		];
		return list;
	}
}

public static class BoardgameTypes
{
    public const string ORIGINAL = "ORIGINAL";
    public const string CHINA_CLONE = "CHINA_CLONE";

    public static string GetDisplayText(string type)
    {
        return type switch
        {
            ORIGINAL => "Original",
            CHINA_CLONE => "Clone (China)",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
		Dictionary<string, string> list = new()
		{
            { ORIGINAL, GetDisplayText(ORIGINAL) },
            { CHINA_CLONE, GetDisplayText(CHINA_CLONE) },
        };
        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem() { Key = ORIGINAL, Value = GetDisplayText(ORIGINAL) },
            new DropdownSelectItem() { Key = CHINA_CLONE, Value = GetDisplayText(CHINA_CLONE) },
        ];
        return list;
    }
}

public static class MerchantTypes
{
    public const string CAFE = "CAFE";
    public const string ECOMMERCE = "ECOMMERCE";
    public const string ONLINE_SHOP = "ONLINE-SHOP";
    public const string PHAMARCY = "PHARMACY";
    public const string RESTAURANT = "RESTAURANT";
    public const string SHOP = "SHOP";
    public const string SHOPPING_MALL = "SHOPPING-MALL";


    public static string GetDisplayText(string? code)
    {
        return code switch
        {
            CAFE => "Cafe",
            ECOMMERCE => "E-Commerce Site",
            ONLINE_SHOP => "Online-Shop",
            PHAMARCY => "Pharmacy",
            RESTAURANT => "Restaurant",
            SHOP => "Shop",
            SHOPPING_MALL => "Shopping Mall",
            _ => "-",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
		Dictionary<string, string> list = new()
		{
            { CAFE, GetDisplayText(CAFE) },
            { ECOMMERCE, GetDisplayText(ECOMMERCE) },
            { ONLINE_SHOP, GetDisplayText(ONLINE_SHOP) },
            { PHAMARCY, GetDisplayText(PHAMARCY) },
            { RESTAURANT, GetDisplayText(RESTAURANT) },
            { SHOP, GetDisplayText(SHOP) },
            { SHOPPING_MALL, GetDisplayText(SHOPPING_MALL) },
        };
        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem() { Key = CAFE, Value = GetDisplayText(CAFE) },
			new DropdownSelectItem() { Key = ECOMMERCE, Value = GetDisplayText(ECOMMERCE) },
			new DropdownSelectItem() { Key = PHAMARCY, Value = GetDisplayText(PHAMARCY) },
			new DropdownSelectItem() { Key = RESTAURANT, Value = GetDisplayText(RESTAURANT) },
			new DropdownSelectItem() { Key = ONLINE_SHOP, Value = GetDisplayText(ONLINE_SHOP) },
			new DropdownSelectItem() { Key = SHOP, Value = GetDisplayText(SHOP) },
			new DropdownSelectItem() { Key = SHOPPING_MALL, Value = GetDisplayText(SHOPPING_MALL) }
		];
		return list;
    }
}

// FOR HOME INVENTORY MANAGEMENT
public static class OwnedItemStateController
{
	public static Dictionary<string, string> GetNextValidActions(string? currentState)
	{
		return currentState switch
		{
			ObjectStates.DRAFT => new()
				{
					{ ObjectStateActions.PURCHASE, ObjectStateActions.GetDisplayText(ObjectStateActions.PURCHASE) },
					{ ObjectStateActions.ADD_WISHLIST, ObjectStateActions.GetDisplayText(ObjectStateActions.ADD_WISHLIST) },
				},
			ObjectStates.NEW => new()
				{
					{ ObjectStateActions.SAVE_DRAFT, ObjectStateActions.GetDisplayText(ObjectStateActions.SAVE_DRAFT) },
					{ ObjectStateActions.PURCHASE, ObjectStateActions.GetDisplayText(ObjectStateActions.PURCHASE) },
					{ ObjectStateActions.ADD_WISHLIST, ObjectStateActions.GetDisplayText(ObjectStateActions.ADD_WISHLIST) },
				},
			ObjectStates.BNIB => new()
				{
					{ ObjectStateActions.START_USING, ObjectStateActions.GetDisplayText(ObjectStateActions.START_USING) },
					{ ObjectStateActions.GIVEN_OTHER, ObjectStateActions.GetDisplayText(ObjectStateActions.GIVEN_OTHER) },
					{ ObjectStateActions.LEND_OUT, ObjectStateActions.GetDisplayText(ObjectStateActions.LEND_OUT) },
				},
			ObjectStates.IN_USE => new()
				{
					{ ObjectStateActions.BROKE, ObjectStateActions.GetDisplayText(ObjectStateActions.BROKE) },
					{ ObjectStateActions.GIVEN_OTHER, ObjectStateActions.GetDisplayText(ObjectStateActions.GIVEN_OTHER) },
					{ ObjectStateActions.LEND_OUT, ObjectStateActions.GetDisplayText(ObjectStateActions.LEND_OUT) },
					{ ObjectStateActions.WRITE_OFF, ObjectStateActions.GetDisplayText(ObjectStateActions.WRITE_OFF) }
				},
			ObjectStates.RARELY_USED => new()
				{
					{ ObjectStateActions.BROKE, ObjectStateActions.GetDisplayText(ObjectStateActions.BROKE) },
					{ ObjectStateActions.GIVEN_OTHER, ObjectStateActions.GetDisplayText(ObjectStateActions.GIVEN_OTHER) },
					{ ObjectStateActions.LEND_OUT, ObjectStateActions.GetDisplayText(ObjectStateActions.LEND_OUT) },
					{ ObjectStateActions.WRITE_OFF, ObjectStateActions.GetDisplayText(ObjectStateActions.WRITE_OFF) }
				},
			ObjectStates.LOANED => new()
			{
				{ ObjectStateActions.RETURN, ObjectStateActions.GetDisplayText(ObjectStateActions.RETURN) },
				{ ObjectStateActions.LOSE, ObjectStateActions.GetDisplayText(ObjectStateActions.LOSE) },
				{ ObjectStateActions.WRITE_OFF, ObjectStateActions.GetDisplayText(ObjectStateActions.WRITE_OFF) }
			},
			ObjectStates.WISHLIST => new()
			{
				{ ObjectStateActions.PURCHASE, ObjectStateActions.GetDisplayText(ObjectStateActions.PURCHASE) }
			},
			_ => []
		};
	}

	public static string? GetResultingTargetState(string currentState, string action)
	{
		string s = currentState + "|" + action;
		return s switch
		{
			ObjectStates.NEW + "|" + ObjectStateActions.SAVE_DRAFT => ObjectStates.DRAFT,
			ObjectStates.NEW + "|" + ObjectStateActions.ADD_WISHLIST => ObjectStates.WISHLIST,
			ObjectStates.NEW + "|" + ObjectStateActions.PURCHASE => ObjectStates.BNIB,
			ObjectStates.DRAFT + "|" + ObjectStateActions.PURCHASE => ObjectStates.BNIB,
			ObjectStates.WISHLIST + "|" + ObjectStateActions.PURCHASE => ObjectStates.BNIB,
			ObjectStates.BNIB + "|" + ObjectStateActions.START_USING => ObjectStates.IN_USE,
			ObjectStates.IN_USE + "|" + ObjectStateActions.BROKE => ObjectStates.BROKEN,
			ObjectStates.IN_USE + "|" + ObjectStateActions.WRITE_OFF => ObjectStates.WRITTEN_OFF,
			ObjectStates.IN_USE + "|" + ObjectStateActions.LEND_OUT => ObjectStates.LOANED,
			ObjectStates.RARELY_USED + "|" + ObjectStateActions.LEND_OUT => ObjectStates.LOANED,
			ObjectStates.RARELY_USED + "|" + ObjectStateActions.BROKE => ObjectStates.BROKEN,
			ObjectStates.RARELY_USED + "|" + ObjectStateActions.WRITE_OFF => ObjectStates.WRITTEN_OFF,
			_ => null
		};
	}

	public static Dictionary<string, string> GetAll()
	{
		Dictionary<string, string> list = new()
		{
			{ ObjectStates.IN_USE, ObjectStates.GetDisplayText(ObjectStates.IN_USE) },
			{ ObjectStates.WISHLIST, ObjectStates.GetDisplayText(ObjectStates.WISHLIST) },
			{ ObjectStates.LOST, ObjectStates.GetDisplayText(ObjectStates.LOST) },
			{ ObjectStates.LOANED, ObjectStates.GetDisplayText(ObjectStates.LOANED) }
		};
		return list;
	}

	public static List<DropdownSelectItem> GetForDropdownSelect()
	{
		List<DropdownSelectItem> list =
		[
			new DropdownSelectItem() { Key = ObjectStates.IN_USE, Value = ObjectStates.GetDisplayText(ObjectStates.IN_USE) },
			new DropdownSelectItem() { Key = ObjectStates.DRAFT, Value = ObjectStates.GetDisplayText(ObjectStates.DRAFT) },
			new DropdownSelectItem() { Key = ObjectStates.WISHLIST, Value = ObjectStates.GetDisplayText(ObjectStates.WISHLIST) },
			new DropdownSelectItem() { Key = ObjectStates.LOST, Value = ObjectStates.GetDisplayText(ObjectStates.LOST) },
			new DropdownSelectItem() { Key = ObjectStates.SOLD, Value = ObjectStates.GetDisplayText(ObjectStates.SOLD) },
			new DropdownSelectItem() { Key = ObjectStates.LOANED, Value = ObjectStates.GetDisplayText(ObjectStates.LOANED) },
		];
		return list;
	}
}

public static class OwnedItemStatuses
{
    public const string BRAND_NEW_IN_BOX = "BNIB";
    public const string IN_USE = "IN-USE";
    public const string RARELY_USE = "RARELY-USE";
    public const string GIFTED_TO_OTHER = "GIFTED";
    public const string SOLD = "SOLD";
    public const string BROKEN = "BROKEN";
    public const string WRITTEN_OFF = "WRITTEN-OFF";
    public const string OTHERS = "OTHER";
    public const string UNKNOWN = "UNKNOWN";

    public static string GetDesc(string code)
    {
        return code switch
        {
            BRAND_NEW_IN_BOX => "Brand New (In Box)",
            IN_USE => "In-Use",
            RARELY_USE => "Rarely Used",
            GIFTED_TO_OTHER => "Given To Others",
            SOLD => "Sold",
            BROKEN => "Broken",
            WRITTEN_OFF => "Written-Off",
            OTHERS => "Belong To Others",
            UNKNOWN => "Unknown",
            _ => "",
        };
    }

    public static Dictionary<string, string> GetAll()
    {
        Dictionary<string, string> list = new Dictionary<string, string>
        {
            { BRAND_NEW_IN_BOX, GetDesc(BRAND_NEW_IN_BOX) },
            { IN_USE, GetDesc(IN_USE) },
            { RARELY_USE, GetDesc(RARELY_USE) },
            { GIFTED_TO_OTHER, GetDesc(GIFTED_TO_OTHER) },
            { SOLD, GetDesc(SOLD) },
            { BROKEN, GetDesc(BROKEN) },
            { WRITTEN_OFF, GetDesc(WRITTEN_OFF) },
            { OTHERS, GetDesc(OTHERS) },
            { UNKNOWN, GetDesc(UNKNOWN) },
        };
        return list;
    }

    public static List<DropdownSelectItem> GetForDropdownSelect()
    {
        List<DropdownSelectItem> list =
        [
            new DropdownSelectItem() { Key = BRAND_NEW_IN_BOX, Value = GetDesc(BRAND_NEW_IN_BOX) },
			new DropdownSelectItem() { Key = IN_USE, Value = GetDesc(IN_USE) },
			new DropdownSelectItem() { Key = RARELY_USE, Value = GetDesc(RARELY_USE) },
			new DropdownSelectItem() { Key = GIFTED_TO_OTHER, Value = GetDesc(GIFTED_TO_OTHER) },
			new DropdownSelectItem() { Key = SOLD, Value = GetDesc(SOLD) },
			new DropdownSelectItem() { Key = BROKEN, Value = GetDesc(BROKEN) },
			new DropdownSelectItem() { Key = WRITTEN_OFF, Value = GetDesc(WRITTEN_OFF) },
			new DropdownSelectItem() { Key = OTHERS, Value = GetDesc(OTHERS) },
			new DropdownSelectItem() { Key = UNKNOWN, Value = GetDesc(UNKNOWN) }
		];

		return list;
    }
}
#endregion