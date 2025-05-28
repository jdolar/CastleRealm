using Shared.Tools;
namespace DataBase.Collections.Castles;
public sealed class TestData(CastleContext context)
{
    public async Task<int> AddRandomCastles(int number)
    {
        RandomGenerator gen = new();
        Data.Add dbAdd = new(context);

        for (int i = 0; i < number; i++)
        {
            await dbAdd.Single(new Shared.Requests.Castles.Add()
            {
                Name = gen.NextString(10),
                Description = gen.NextString(10),
                Note = gen.NextString(10),
                Type = gen.NextString(10),
                Url = string.Format("http://www.{0}.{1}", gen.NextString(10), gen.NextString(3)),
                Country = gen.NextString(10),
                Region = gen.NextString(10),
                Town = gen.NextString(10),
                State = gen.NextString(10),
                Location = string.Format("{0},{1}", gen.NextInt(0, 100), gen.NextInt(0, 100))
            });
        }

        return 0;
    }
}
