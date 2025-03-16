using DataServiceLib;
using FlagpoleCRM.Models;
using Moq;
using RepositoriesLib;

namespace CustomerServiceTest
{
    [TestClass]
    public class CustomerServiceTest
    {
        private readonly CustomerService _sut;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock = new Mock<ICustomerRepository>();

        public CustomerServiceTest()
        {
            _sut = new CustomerService(_customerRepositoryMock.Object);
        }
        [TestMethod]
        public void GetAudiences_ShouldReturnAudienceList()
        {
            var websiteId = new Guid().ToString();
            var audiences = new List<Audience>
            {
                new Audience
                {
                    Id = new Guid().ToString(),
                    WebsiteId = websiteId
                },
                new Audience
                {
                    Id = new Guid().ToString(),
                    WebsiteId = websiteId
                }
            };

            _customerRepositoryMock.Setup(x => x.GetAudiences(websiteId)).Returns(audiences);
            var result = _sut.GetAudiences(websiteId);

            Assert.IsNotNull(result);
        }
    }
}