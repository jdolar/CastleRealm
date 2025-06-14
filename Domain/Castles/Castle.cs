using DataBase.Collections.Castles;
namespace Domain.Castles;
public class Castle(CastleContext dbContext)
{
    public async Task<int> Add(Shared.Requests.Castles.Add request)
    {
        DataBase.Collections.Castles.Data.Add add = new(dbContext);
        int castleId = (int)await add.Single(request);
        return castleId;
    }
    public async Task<int> AddTestData(int number)
    {
        TestData data = new(dbContext);
        await data.AddRandomCastles(number);
        return 1;
    }
    public async Task<bool> Delete(int? id = null, string? name = null)
    {
        DataBase.Collections.Castles.Data.Delete delete = new(dbContext);      
        return await delete.Single(id, name);
    }
    public async Task<List<DataBase.Collections.Castles.Models.Castle>> Get(int? id = null, string? name = null)
    {
        DataBase.Collections.Castles.Data.Get get = new(dbContext);
        var castle = await get.ByIdName(id, name);
        return castle;
    }
}
