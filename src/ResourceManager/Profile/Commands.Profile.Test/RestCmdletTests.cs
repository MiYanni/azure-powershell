using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.Common.Authentication.Models;
using Microsoft.Azure.Commands.Profile.Models;
using Microsoft.Azure.Commands.ScenarioTest;
using Microsoft.Azure.ServiceManagemenet.Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;
using Assert = Xunit.Assert;

namespace Microsoft.Azure.Commands.Profile.Test
{
    public class RestCmdletTests
    {
        //private MemoryDataStore dataStore;
        //private MockCommandRuntime commandRuntimeMock;

        public RestCmdletTests(ITestOutputHelper output)
        {
            TestExecutionHelpers.SetUpSessionAndProfile();
            XunitTracingInterceptor.AddToContext(new XunitTracingInterceptor(output));
            //dataStore = new MemoryDataStore();
            //AzureSession.Instance.DataStore = dataStore;
            //commandRuntimeMock = new MockCommandRuntime();
            //AzureRmProfileProvider.Instance.Profile = new AzureRmProfile();
        }

        public static IEnumerable<object[]> UriDataPositive => new []
        {
            new object[] 
            {
                "https://management.azure.com/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10",
                "https://management.azure.com/",
                "https://management.azure.com/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10" 
            },
            new object[] 
            {
                "/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10",
                "https://management.azure.com/",
                "https://management.azure.com/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10" 
            },
            new object[] 
            {
                "subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10",
                "https://management.azure.com/",
                "https://management.azure.com/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10" 
            }
        };

        [Theory, MemberData(nameof(UriDataPositive))]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void ResolveUriPositive(string uri, string azureRmUri, string result)
        {
            //var target = new InvokeAzureRmRestMethod();
            //// https://stackoverflow.com/a/15607491/294804
            //var obj = new PrivateObject(target);
            //var retVal = obj.Invoke("ResolveAzureUri", new Uri(uri), new Uri(azureRmUri)).ToString();
            //Assert.Equal(retVal, result);


            var uriType = Uri.IsWellFormedUriString(uri, UriKind.Absolute) ? UriKind.Absolute : UriKind.Relative;
            // https://stackoverflow.com/questions/9122708/unit-testing-private-methods-in-c-sharp#comment74512266_15607491
            var pt = new PrivateType(typeof(InvokeAzureRmRestMethod));
            var retVal = pt.InvokeStatic("ResolveAzureUri", new Uri(uri, uriType), new Uri(azureRmUri)).ToString();
            Assert.Equal(retVal, result);
        }

        public static IEnumerable<object[]> UriDataNegative => new[]
        {
            new object[] 
            {
                "fdas5f4#$@*(/dsa/fdsa1ds/a65134543$#!@$\\&$%#&SDAfdhsgfdsg\\/jfhdas832541#/&(#@!(&DS()&VHvcxhhs",
                "https://management.azure.com/" 
            },
            new object[] 
            {
                "http://management.azure.com/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10",
                "https://management.azure.com/" 
            },
            new object[] 
            {
                "https://management.core.windows.net/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources?api-version=2017-05-10",
                "https://management.azure.com/" 
            },
            new object[] 
            {
                "https://management.azure.com/subscriptions/c9cbd920-c00c-427c-852b-8aaf38badaeb/resources",
                "https://management.azure.com/" 
            }
        };

        [Theory, MemberData(nameof(UriDataNegative))]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void ResolveUriNegative(string uri, string azureRmUri)
        {
            var uriType = Uri.IsWellFormedUriString(uri, UriKind.Absolute) ? UriKind.Absolute : UriKind.Relative;
            // https://stackoverflow.com/questions/9122708/unit-testing-private-methods-in-c-sharp#comment74512266_15607491
            var pt = new PrivateType(typeof(InvokeAzureRmRestMethod));
            Assert.Throws<PSArgumentException>(() => pt.InvokeStatic("ResolveAzureUri", new Uri(uri, uriType), new Uri(azureRmUri)));
        }

        //[Fact]
        //[Trait(Category.RunType, Category.LiveOnly)]
        //public void SendFeedbackFailsInNonInteractive()
        //{
        //    var cmdlet = new SendFeedbackCommand();

        //    // Setup
        //    cmdlet.CommandRuntime = commandRuntimeMock;

        //    // Act
        //    Assert.ThrowsAny<Exception>(() =>
        //    {
        //        cmdlet.InvokeBeginProcessing();
        //    });
        //}

        //[Fact]
        //[Trait(Category.AcceptanceType, Category.CheckIn)]
        //public void CanSerializeSimpleFeedbackPayloadIntoProperForm()
        //{
        //    var payload = new PSAzureFeedback
        //    {
        //        ModuleName = "Module",
        //        ModuleVersion = "1.0.0",
        //        SubscriptionId = Guid.NewGuid().ToString(),
        //        TenantId = Guid.NewGuid().ToString(),
        //        Environment = "AzureCloud",
        //        Recommendation = 10,
        //        PositiveComments = "Positive",
        //        NegativeComments = "Negative",
        //        Email = "m@e.com"
        //    };

        //    var serializedPayload = MetricHelper.SerializeCustomEventPayload(payload);

        //    Assert.Equal(payload.ModuleName, serializedPayload["moduleName"]);
        //    Assert.Equal(payload.ModuleVersion, serializedPayload["moduleVersion"]);
        //    Assert.Equal(payload.SubscriptionId.ToString(), serializedPayload["subscriptionId"]);
        //    Assert.Equal(payload.TenantId.ToString(), serializedPayload["tenantId"]);
        //    Assert.Equal(payload.Environment, serializedPayload["environment"]);
        //    Assert.Equal(payload.Recommendation.ToString(), serializedPayload["recommendation"]);
        //    Assert.Equal(payload.PositiveComments, serializedPayload["positiveComments"]);
        //    Assert.Equal(payload.NegativeComments, serializedPayload["negativeComments"]);
        //    Assert.Equal(payload.Email, serializedPayload["email"]);
        //}
    }
}
