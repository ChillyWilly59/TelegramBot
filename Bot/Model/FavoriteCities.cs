using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Model
{
    public class FavoriteCities
    {
        [Key]
        public int Id { get; set; }
        public long UserId { get; set; }
        public List<string> CityNames { get; set; } = [];

        public FavoriteCities()
        {
        }

        public void Initialize(long userId, string name)
        {
            UserId = userId;
            CityNames.Add(name);
        }

        public string GetDisplay() => $"Город: {CityNames}";
    }

}
