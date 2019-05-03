using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Api.Dynamic
{
    public class User
    {
        public int Id { set; get; }
        public string Name { set; get; }
    }

    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetById(int id);
    }

    public class DbUserRepository : IUserRepository
    {
        public IEnumerable<User> GetAll()
        {
            return new User[] { new User() { Id = 1, Name = Guid.NewGuid().ToString() } };
        }

        public User GetById(int id)
        {
            return new User() { Id = 1, Name = Guid.NewGuid().ToString() };
        }
    }

    public class UserController : ApiController
    {
        private readonly IUserRepository repository;

        // Use constructor injection here
        public UserController(IUserRepository repository)
        {
            this.repository = repository;
        }

        [Route("api/user/all")]
        [HttpGet]
        public IEnumerable<User> GetAllUsers() => this.repository.GetAll();

        [Route("api/user/{id}")]
        [HttpGet]
        public User GetUserById(int id)
        {
            try
            {
                return this.repository.GetById(id);
            }
            catch (KeyNotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
} 