using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Domain.Search;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Search;

public class SearchRepository : ISearchRepository
{
    private readonly ApplicationDbContext _context;

    public SearchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Item> items, int totalCount)> SearchItemsAsync(SearchQuery query, CancellationToken ct)
    {
        var searchTerm = query.SearchTerm.Replace("'", "''"); // SQL escape

        // Build the search query with filters
        var sql = $@"
            SELECT i.*
            FROM dbo.Items i
            INNER JOIN FREETEXTTABLE(dbo.Items, (Name, Notes, Attributes), '{searchTerm}') AS ft ON i.ItemId = ft.[KEY]
            WHERE i.IsDeleted = 0
              AND (@CollectionId IS NULL OR i.CollectionId = @CollectionId)
              AND (@StartDate IS NULL OR i.AcquisitionDate >= @StartDate)
              AND (@EndDate IS NULL OR i.AcquisitionDate <= @EndDate)
            ORDER BY ft.RANK DESC
            OFFSET {query.Skip} ROWS FETCH NEXT {query.Take} ROWS ONLY;
        ";

        var items = await _context.Items
            .FromSqlRaw(sql,
                new SqlParameter("@CollectionId", (object?)query.CollectionId ?? DBNull.Value),
                new SqlParameter("@StartDate", (object?)query.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object?)query.EndDate ?? DBNull.Value))
            .AsNoTracking()
            .ToListAsync(ct);

        // Get total count (without pagination)
        var countSql = $@"
            SELECT COUNT(*)
            FROM dbo.Items i
            INNER JOIN FREETEXTTABLE(dbo.Items, (Name, Notes, Attributes), '{searchTerm}') AS ft ON i.ItemId = ft.[KEY]
            WHERE i.IsDeleted = 0
              AND (@CollectionId IS NULL OR i.CollectionId = @CollectionId)
              AND (@StartDate IS NULL OR i.AcquisitionDate >= @StartDate)
              AND (@EndDate IS NULL OR i.AcquisitionDate <= @EndDate);
        ";

        var totalCount = await _context.Database.SqlQueryRaw<int>(countSql,
            new SqlParameter("@CollectionId", (object?)query.CollectionId ?? DBNull.Value),
            new SqlParameter("@StartDate", (object?)query.StartDate ?? DBNull.Value),
            new SqlParameter("@EndDate", (object?)query.EndDate ?? DBNull.Value))
            .FirstOrDefaultAsync(ct);

        return (items, totalCount);
    }

    public async Task<string[]> GetSuggestionsAsync(string prefix, Guid userId, int limit, CancellationToken ct)
    {
        return await _context.Items
            .Where(i => !i.IsDeleted && i.Name.Value.StartsWith(prefix))
            .Select(i => i.Name.Value)
            .Distinct()
            .OrderBy(n => n)
            .Take(limit)
            .ToArrayAsync(ct);
    }
}
