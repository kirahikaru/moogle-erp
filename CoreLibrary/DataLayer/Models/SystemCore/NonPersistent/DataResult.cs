namespace DataLayer.Models.SystemCore.NonPersistent;

public class DataResult<T>
{
    public DataPagination? Pagination { set; get; }
    public List<T> Records { set; get; }

    public DataResult(List<T> records, DataPagination dataPagination)
    {
        Pagination = dataPagination;
        Records = records;
    }

    public DataResult(List<T> records)
    {
        Pagination = null;
        Records = records;
    }

    public DataResult(string objectType, List<T> records, int pageNo, int pageSize, decimal totalCount)
    {
        SetPagingResult(objectType, pageNo, pageSize, totalCount);
        Records = records;
    }

    public DataResult()
    {
        Records = new();
    }

    public void SetPagingResult(string objectType, int pageNo, int pageSize, decimal totalCount)
    {
        int pageCount = (int)(Math.Ceiling(totalCount / (decimal)pageSize));
        Pagination = new DataPagination()
        {
            ObjectType = objectType,
            PageNo = pageNo,
            PageSize = pageSize,
            PageCount = pageCount,
            RecordCount = (int)totalCount
        };
    }
}
