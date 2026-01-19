using flea_WebProj.Services;

namespace flea_WebProj.Tests.Services
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void Verify_ShouldReturnTrue_ForCorrectPassword()
        {
            const string password = "SecurePass!";
            var hash = _hasher.Hash(password);

            var result = _hasher.Verify(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void Verify_ShouldReturnFalse_ForIncorrectPassword()
        {
            var hash = _hasher.Hash("Correct");
            var result = _hasher.Verify("Incorrect", hash);

            Assert.False(result);
        }

        [Fact]
        public void Verify_ShouldReturnFalse_ForInvalidHash()
        {
            var result = _hasher.Verify("password", "not-base64");

            Assert.False(result);
        }
    }
}