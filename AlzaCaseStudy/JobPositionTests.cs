using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using AlzaCaseStudy.Models;
using AlzaCaseStudy.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using Serilog;

namespace AlzaCaseStudy
{
    [TestFixture]
    public class JobPositionTests
    {
        private StringBuilder stringBuilder; 
        private ILogger _logger;
        private HttpClient _client;

        public string ParseUri(string uri)
        {
            string[] parsedUri = uri.Split(Consts.API_PREFIX);
            if (parsedUri.Length == 2)
                return parsedUri[1];
            _logger.Error($"Wrong uri format: {uri}");
            return "";
        }

        public async Task<ResponseDto<T>> GetDataAsync<T>(string uri)
        {
            string apiRequestAddress = ParseUri(uri);
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/{apiRequestAddress}");
            request.Headers.UserAgent.ParseAdd(Consts.HEADER_INFO);
            _logger.Information($"Requesting data from address: {request.RequestUri}");
            var response = await _client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.Information($"Succesfully recieved data, deserializing...");
                return new ResponseDto<T>
                {
                    StatusCode = response.StatusCode,
                    Response = JsonConvert.DeserializeObject<T>(content)
                };
            }
            else
            {
                _logger.Error($"Request failed with status code: {response.StatusCode}");
                _logger.Error($"Response content: {await response.Content.ReadAsStringAsync()}");
                return new ResponseDto<T>
                {
                    StatusCode = response.StatusCode,
                    Message = $"Didn't manage to get data from the api: {response.StatusCode}"
                };
            }
        }


        [OneTimeSetUp]
        public void SetUp()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(Consts.BASE_API_URI);
            if (!File.Exists(Consts.CONFIG_PATH))
                return;
            var conf = new ConfigurationBuilder().AddJsonFile(Consts.CONFIG_PATH).Build();
            _logger = new LoggerConfiguration().ReadFrom.Configuration(conf).CreateLogger() as ILogger;
            stringBuilder = new StringBuilder();
            Log.Logger = _logger;
        }        

        [OneTimeTearDown]
        public async Task TearDown()
        {
            File.WriteAllText(Consts.RESULT_PATH,stringBuilder.ToString());
            await Log.CloseAndFlushAsync();
            _client.Dispose();            
        }

        [Test]
        public async Task Test_PositionAndPositionDescriptionShouldBeFilled()
        {
            _logger.Information("Starting test for description");
            stringBuilder.AppendLine($"------------------Checking Position and Position Description------------------");
            var positionData = await GetDataAsync<PositionResponse>(Consts.MAIN_JSON_API_ADDRESS);
            _logger.Information($"Deserialized, asserting.");
            Assert.That(positionData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.PositionItems);
            Assert.IsNotNull(positionData.Response.PositionItems.Meta);
            Assert.IsNotNull(positionData.Response.PositionItems.Meta.Href);

            var positionDescItems = await GetDataAsync<PositionItems>(positionData.Response.PositionItems.Meta.Href);
            _logger.Information($"Deserialized, asserting.");
            Assert.That(positionDescItems.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(positionDescItems.Response);
            Assert.IsNotNull(positionDescItems.Response.items);
            bool pass = positionDescItems.Response.items
                .Any(item => item.SubContent.Any(subContent => !string.IsNullOrEmpty(subContent)));
            Assert.IsTrue(pass);
            _logger.Information($"Finished asserting.");

            stringBuilder.AppendLine("Position description: ");
            foreach (var item in positionDescItems.Response.items)
            {
                foreach (var subContent in item.SubContent)
                {
                    stringBuilder.AppendLine("  " + subContent);
                }
            }
            _logger.Information("Description test finished");
        }

        [Test]
        public async Task Test_PlaceOfEmploymentShouldBeFilled()
        {
            _logger.Information("Starting test for place of employment");
            stringBuilder.AppendLine($"------------------Checking what is place of emplyment------------------");
            var positionData = await GetDataAsync<PositionResponse>(Consts.MAIN_JSON_API_ADDRESS);
            _logger.Information($"Deserialized, asserting.");
            Assert.That(positionData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.PlaceOfEmplyment);
            Assert.IsNotNull(positionData.Response.Department.Name);
            _logger.Information($"Finished asserting.");

            stringBuilder.AppendLine($"Place Of Emplyment: {positionData.Response.PlaceOfEmplyment}");
            _logger.Information("Place of employment test finished");
        }

        [Test]
        public async Task Test_GestorUserAndExecutiveUserShouldBeFilled()
        {
            _logger.Information("Starting test for interviewers");
            stringBuilder.AppendLine($"------------------Checking GestorUser and ExecutiveUser data------------------");
            var positionData = await GetDataAsync<PositionResponse>(Consts.MAIN_JSON_API_ADDRESS);
            _logger.Information($"Deserialized, asserting.");
            Assert.That(positionData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.GestorUser);
            Assert.IsNotNull(positionData.Response.ExecutiveUser);
            Assert.IsNotEmpty(positionData.Response.GestorUser.Meta.Href);
            Assert.IsNotEmpty(positionData.Response.ExecutiveUser.Meta.Href);
            _logger.Information($"Finished asserting.");

            var gestorUserData = await GetDataAsync<EmployeeResponse>(positionData.Response.GestorUser.Meta.Href);
            _logger.Information($"Deserialized, asserting.");
            Assert.That(gestorUserData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(gestorUserData.Response);
            Assert.IsNotEmpty(gestorUserData.Response.Name);
            Assert.IsNotEmpty(gestorUserData.Response.Image);
            Assert.IsNotEmpty(gestorUserData.Response.Description);            
            _logger.Information($"Finished asserting.");

            var executiveUserData = await GetDataAsync<EmployeeResponse>(positionData.Response.ExecutiveUser.Meta.Href);
            _logger.Information($"Deserialized, asserting.");
            Assert.That(executiveUserData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(executiveUserData.Response);
            Assert.IsNotEmpty(executiveUserData.Response.Name);
            Assert.IsNotEmpty(executiveUserData.Response.Image);
            Assert.IsNotEmpty(executiveUserData.Response.Description);
            _logger.Information($"Finished asserting.");

            stringBuilder.AppendLine($"GestorUser Name: {gestorUserData.Response.Name}");
            stringBuilder.AppendLine($"GestorUser Image: {gestorUserData.Response.Image}");
            stringBuilder.AppendLine($"GestorUser Description: {gestorUserData.Response.Description}");
            stringBuilder.AppendLine($"ExecutiveUser Name: {executiveUserData.Response.Name}");
            stringBuilder.AppendLine($"ExecutiveUser Image: {executiveUserData.Response.Image}");
            stringBuilder.AppendLine($"ExecutiveUser Description: {executiveUserData.Response.Description}");
            _logger.Information("Interviewers test finished");
        }

        [Test]
        public async Task Test_SuitableForStudensFieldShouldBeFilled()
        {
            _logger.Information("Starting test for students");
            stringBuilder.AppendLine($"------------------Checking if position is suitable for students------------------");
            var positionData = await GetDataAsync<PositionResponse>(Consts.MAIN_JSON_API_ADDRESS);
            _logger.Information($"Deserialized, asserting.");
            Assert.That(positionData.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(positionData.Response);
            Assert.IsNotNull(positionData.Response.ForStudents);
            _logger.Information($"Finished asserting.");

            stringBuilder.AppendLine($"Suitable for students: {positionData.Response.ForStudents}");
            _logger.Information("Students test finished");
        }        
    }
}