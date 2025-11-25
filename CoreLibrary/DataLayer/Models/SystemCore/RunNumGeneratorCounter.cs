using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Models.SystemCore;

[DisplayName("Running Number Generator Counter")]
//[Table("[dbo].[RunningNumberGeneratorCounter]")]
[Table("RunNumGenCounter")]
public class RunNumGeneratorCounter : AuditObject
{
	[Computed, ReadOnly(true), Write(false)]
	public new static string MsSqlTableName => $"RunNumGenCounter";

	[Computed, ReadOnly(true), Write(false)]
	public new static string PgTableName => $"run_num_gen_counter";

	[Computed, Write(false), ReadOnly(true)]
	public static string MsSqlTable => DatabaseObj.GetTable(SchemaName, MsSqlTableName, DatabaseTypes.MSSQL);

	[Computed, Write(false), ReadOnly(true)]
	public static string PgTable => DatabaseObj.GetTable(SchemaName, PgTableName, DatabaseTypes.POSTGRESQL);

	[Computed, Write(false), ReadOnly(true)]
	public static DatabaseObj DatabaseObject => new(SchemaName, MsSqlTableName, PgTableName);

	#region *** DATABASE FIELDS ***
	public int? RunningNumberGeneratorId { get; set; }
    [Range(0, 9999999999, ErrorMessage = "'CurrentNumber' invalid format. Only positive number is allowed.")]
    public int CurrentNumber { get; set; }

    [Range(1, 9999, ErrorMessage = "'Interval Year' invalid format. Only positive number is allowed.")]
    public int? IntervalYear { get; set; }

    [Range(1, 4, ErrorMessage = "'Interval Quarter' invalid format. Only positive number is allowed.")]
    public int? IntervalQuarter { get; set; }

    [Range(1, 12, ErrorMessage = "'Interval Month' invalid format. Only positive number is allowed.")]
    public int? IntervalMonth { get; set; }

    [Range(1, 31, ErrorMessage = "'Interval Day' invalid format. Only positive number is allowed.")]
    public int? IntervalDay { get; set; }
    public bool IsCurrent { get; set; }
    #endregion

    #region *** LINKED OBJECTS ***

    [Write(false), Computed]
    public RunNumGenerator? RunningNumberGenerator { get; set; }
    #endregion

    #region *** DYNAMIC PROPERTIES ***
    [Write(false), Computed]
    [Description("ignore"), ReadOnly(true)]
    public string? GeneratedObjectCode
    {
        get
        {
            StringBuilder sb = new();

            if (RunningNumberGenerator is null)
                return null;

            if (RunningNumberGenerator.Prefix.IsAtLeast(1))
                sb.Append(RunningNumberGenerator.Prefix);

            if (IntervalYear.HasValue)
                sb.Append((IntervalYear%10000).Value.ToString("00"));

            if (IntervalQuarter.HasValue)
                sb.Append(IntervalQuarter!.Value.ToString("0"));

            if (IntervalMonth.HasValue)
                sb.Append(IntervalMonth!.Value.ToString("00"));

            if (IntervalDay.HasValue)
                sb.Append(IntervalDay!.Value.ToString("00"));

            switch (RunningNumberGenerator.ResetInterval)
            {
                case SystemIntervals.YEARLY:
                    sb.Append(CurrentNumber < 100000000 ? CurrentNumber!.ToString("00000000") : CurrentNumber!.ToString());
                    break;
                case SystemIntervals.QUARTERLY:
                    sb.Append(CurrentNumber < 1000000 ? CurrentNumber!.ToString("000000") : CurrentNumber!.ToString());
                    break;
                case SystemIntervals.MONTHLY:
                    sb.Append(CurrentNumber < 100000 ? CurrentNumber!.ToString("00000") : CurrentNumber!.ToString());
                    break;
                case SystemIntervals.DAILY:
                    sb.Append(CurrentNumber < 10000 ? CurrentNumber!.ToString("0000") : CurrentNumber!.ToString());
                    break;
                default:
                    sb.Append(CurrentNumber!.ToString("00000000"));
                    break;
            }

            if (RunningNumberGenerator.Suffix.IsAtLeast(1))
                sb.Append(RunningNumberGenerator.Suffix);

            return sb.ToString();
        }
    }
    #endregion
}