using Machine.Specifications;
using System;

namespace CommerceTraining.Tests
{
    [Subject("Authentication")]
    class TestSample
    {
        static SecurityService Subject;
        static UserToken Token;

        Establish context = () =>
            Subject = new SecurityService();

        Because of = () =>
            Token = Subject.Authenticate("username", "password");

        It should_indicate_the_users_role = () =>
            Token.Role.ShouldEqual(Roles.Admin);

        It should_have_a_unique_session_id = () =>
            Token.SessionId.ShouldNotBeNull();
    }


    internal class UserToken
    {
        public object Role { get; internal set; }
        public object SessionId { get; internal set; }
    }

    internal class SecurityService
    {
        internal UserToken Authenticate(string v1, string v2)
        {
            return new UserToken { Role = "Admin", SessionId = Guid.NewGuid() };
        }
    }

    internal class Roles
    {
        internal const string Admin = "Admin";
    }
}
