using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.UnitTests.ApplicationsTests
{
    [TestClass]
    public class ResetEnvironmentsSortOrderTests
    {
        private InMemoryApplicationRepository _applicationRepository;
        private ResetEnvironmentsSortOrderController _resetEnvironmentsSortOrderController;

        [TestInitialize]
        public void BeforeTest()
        {
            _applicationRepository = new InMemoryApplicationRepository();
            _resetEnvironmentsSortOrderController = new ResetEnvironmentsSortOrderController(_applicationRepository);
        }

        [TestMethod]
        public async Task ResetEnvironmentsSortOrder()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _applicationRepository.AddAsync(app);
            app.AddDeployEnvironment("QA", false, false, true);
            app.AddDeployEnvironment("LIVE", false, true, true);

            var environments = app.DeploymentEnvironments;

            //act
            var result = await _resetEnvironmentsSortOrderController.ResetSortOrderForEnvironments();

            //assert
            result.Should().BeOfType<OkObjectResult>("SortOrder for environments has been changed!");
            var resetedEnvironments  = app.DeploymentEnvironments.OrderBy(env => env.SortOrder);
            resetedEnvironments.Should().BeEquivalentTo(environments);
            var firstEnvironment = resetedEnvironments.First();
            firstEnvironment.SortOrder.Should().Be(0);
            var lastEnvironment = resetedEnvironments.Last();
            lastEnvironment.SortOrder.Should().Be(resetedEnvironments.Count() - 1);
        }
    }
}
