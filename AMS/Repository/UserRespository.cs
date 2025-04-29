using AMS.Data;
using AMS.Interfaces;
using AMS.Models;
using Dapper;
using static Dapper.SqlMapper;

namespace AMS.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly IGenericRepository<User> _userGenRepository;

        public UserRepository(IGenericRepository<User> userGenRepository)
        {
            _userGenRepository = userGenRepository;
        }

        public async Task<User> GetByUsername(string username)
        {
            var filters = new Dictionary<string, object>() { { "username", username } };
            return await _userGenRepository.GetFirstAsync("id", filters);
        }




        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userGenRepository.GetAllAsync();
        }




        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _userGenRepository.GetByUsernameAsync(username);
        }

        public Task<int> AddAsync(User user)
        {
            return _userGenRepository.InsertAsync(user);
        }




        //public async Task<User?> GetByIdAsync(string idColumn, int id)
        //{
        //      return await _userGenRepository.GetByIdAsync(idColumn, id);
        //}



        public Task<int> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}






//public async Task<int> DeleteAsync(int id)
//{
//    var query = $"UPDATE [{_tableName}] SET IsDelete = 1 WHERE Id = @Id";
//    using var connection = _context.CreateConnection();
//    return await connection.ExecuteAsync(query, new { Id = id });
//}

//public async Task<T?> GetByIdAsync(int id)
//{
//    var query = $"SELECT * FROM [{_tableName}] WHERE Id = @Id AND IsDelete = 0";
//    using var connection = _context.CreateConnection();
//    return await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
//}

//public async Task<User?> GetByUsernameAsync(string username)
//{
//    var query = $"SELECT * FROM [{_tableName}] WHERE Username = @Username AND IsDelete = 0";
//    using var connection = _context.CreateConnection();
//    return await connection.QueryFirstOrDefaultAsync<User>(query, new { Username = username });
//}



