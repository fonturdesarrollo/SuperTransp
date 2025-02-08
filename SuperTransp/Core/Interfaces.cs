
using SuperTransp.Models;

namespace SuperTransp.Core
{
    public class Interfaces
    {
        public interface ISecurity
        {
            public SecurityUserModel GetValidUser(string login, string password);
        }
    }
}
