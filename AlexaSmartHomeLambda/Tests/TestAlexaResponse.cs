// -*- coding: utf-8 -*-

// Copyright 2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// Licensed under the Amazon Software License (the "License"). You may not use this file except in
// compliance with the License. A copy of the License is located at

//    http://aws.amazon.com/asl/

// or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace AlexaSmartHomeLambda.Tests
{
    [TestFixture]
    public class AlexaResponseUnitTest
    {
        [Test]
        public void Test_Response()
        {
            // Arrange
            string alexaResponseString = new AlexaResponse().ToString();

            // Act
            JObject response = JObject.Parse(alexaResponseString);

            // Assert
            Assert.AreEqual(response["event"]["header"]["namespace"].ToString(), "Alexa");
            Assert.AreEqual(response["event"]["header"]["name"].ToString(), "Response");
        }

        [Test]
        public void Test_Response_Error()
        {
            // Arrange
            JObject payload_error = new JObject();
            payload_error.Add("type", "INVALID_SOMETHING");
            payload_error.Add("message", "ERROR_MESSAGE");
            AlexaResponse ar = new AlexaResponse("Alexa", "ErrorResponse");
            ar.SetPayload(payload_error.ToString());

            // Act
            JObject response = JObject.Parse(ar.ToString());
            System.Console.WriteLine(response);
            
            // Assert
            Assert.AreEqual(response["event"]["header"]["name"].ToString(), "ErrorResponse");
            Assert.AreEqual(response["event"]["payload"]["type"].ToString(), "INVALID_SOMETHING");
            Assert.AreEqual(response["event"]["payload"]["message"].ToString(), "ERROR_MESSAGE");
        }

        [Test]
        public void Test_Discovery()
        {
            // Arrange
            AlexaResponse ar = new AlexaResponse("Alexa.Discovery", "Discover.Response", "endpoint-001");

            JObject capabilityAlexa = JObject.Parse(ar.CreatePayloadEndpointCapability());
            
            JObject propertyPowerstate = new JObject();
            propertyPowerstate.Add("name", "powerState");
            JObject capabilityAlexaPowerController = JObject.Parse(ar.CreatePayloadEndpointCapability("AlexaInterface", "Alexa.PowerController", "3", propertyPowerstate.ToString()));
            
            JArray capabilities = new JArray();
            capabilities.Add(capabilityAlexa);
            capabilities.Add(capabilityAlexaPowerController);
            
            ar.AddPayloadEndpoint("test", capabilities.ToString());

            // Act
            JObject response = JObject.Parse(ar.ToString());
            System.Console.WriteLine(response);
            
            // Assert
            Assert.AreEqual(response["event"]["header"]["namespace"].ToString(), "Alexa.Discovery");
            Assert.AreEqual(response["event"]["header"]["name"].ToString(), "Discover.Response");          
            Assert.AreEqual(response["event"]["payload"]["endpoints"][0]["friendlyName"].ToString(), "Sample Endpoint");
            Assert.AreEqual(response["event"]["payload"]["endpoints"][0]["capabilities"][0]["type"].ToString(), "AlexaInterface");
            Assert.AreEqual(response["event"]["payload"]["endpoints"][0]["capabilities"][0]["interface"].ToString(), "Alexa");
            Assert.AreEqual(response["event"]["payload"]["endpoints"][0]["capabilities"][1]["interface"].ToString(), "Alexa.PowerController");
        }
        
        [Test]
        public void Test_Cookie()
        {
            // Arrange
            AlexaResponse ar = new AlexaResponse();
            // Act
            ar.AddCookie("key", "value");
            JObject response = JObject.Parse(ar.ToString());
            
            // Assert
            Assert.AreEqual(response["event"]["endpoint"]["cookie"]["key"].ToString(), "value");
        }

        [Test]
        public void Test_Cookie_Multiple()
        {
            // Arrange
            AlexaResponse ar = new AlexaResponse();
            
            // Act
            ar.AddCookie("key1", "value1");
            ar.AddCookie("key2", "value2");
            ar.AddCookie("key3", "value3");
            JObject response = JObject.Parse(ar.ToString());
            
            // Assert
            Assert.AreEqual(response["event"]["endpoint"]["cookie"]["key1"].ToString(), "value1");
            Assert.AreEqual(response["event"]["endpoint"]["cookie"]["key2"].ToString(), "value2");
            Assert.AreEqual(response["event"]["endpoint"]["cookie"]["key3"].ToString(), "value3");  
        }

    }

}