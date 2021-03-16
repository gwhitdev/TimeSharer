using System.Collections.Generic;
using TimeSharerApi.Models;
namespace TimeSharerApi.Interfaces
{
    public interface IUsersService
    {
        public List<User> Get();
        public User Create(User userIn);
        public User Get(string id);
        public bool Update(string id, UserDetails userDetailsIn);
        public bool Delete(string id);
    }
}
