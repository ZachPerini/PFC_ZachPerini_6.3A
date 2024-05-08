using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZachPerini_6._3A_HA.Models;

namespace ZachPerini_6._3A_HA.Repositories
{
    public class RedisRepository
    {
        private readonly IDatabase _db;

        public RedisRepository(string redisConnection)
        {
            //var connection = ConnectionMultiplexer.Connect("redis-18900.c1.us-central1-2.gce.redns.redis-cloud.com:18900,password=xYEEpwYiWz2x0us8RDmI2xiOTEJfEu0r");
            var connection = ConnectionMultiplexer.Connect(redisConnection);
            _db = connection.GetDatabase();
        }

        public async Task<List<Menu>> GetMenus()
        {
            string menusJson = await _db.StringGetAsync("menus");
            var menus = JsonConvert.DeserializeObject<List<Menu>>(menusJson);
            return menus;
        }

    }
}
