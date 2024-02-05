using Bot.Model;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Services
{
    public class FavoriteCitiesServices(BotDbContext dbContext)
    {
        private readonly BotDbContext _dbContext = dbContext;

        public void AddFavoriteCity(long userId, string name)
        {
            var userFavorites = _dbContext.FavoriteCities.FirstOrDefault(f => f.UserId == userId);

            if (userFavorites == null)
            {
                userFavorites = new FavoriteCities();
                userFavorites.Initialize(userId, name);
                _dbContext.FavoriteCities.Add(userFavorites);
            }

            userFavorites.CityNames.Add(name);
            _dbContext.SaveChanges();
        }

        public void RemoveFavoriteCity(long userId, string cityName)
        {
            var userFavorites = _dbContext.FavoriteCities.FirstOrDefault(f => f.UserId == userId);

            if (userFavorites != null)
            {
                userFavorites.CityNames.Remove(cityName);
                _dbContext.SaveChanges();
            }
        }

        public List<string> GetFavoriteCities(long userId)
        {
            var userFavorites = _dbContext.FavoriteCities.FirstOrDefault(f => f.UserId == userId);

            return userFavorites?.CityNames.ToList() ?? [];
        }
    }

}
