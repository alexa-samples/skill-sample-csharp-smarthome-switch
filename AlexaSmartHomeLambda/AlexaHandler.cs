// -*- coding: utf-8 -*-

// Copyright 2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// Licensed under the Amazon Software License (the "License"). You may not use this file except in
// compliance with the License. A copy of the License is located at

//    http://aws.amazon.com/asl/

// or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

namespace AlexaSmartHomeLambda
{
    public class AlexaHandler
    {   
        public Stream Handler(Stream inputStream, ILambdaContext context)
        {
            StreamReader reader = new StreamReader(inputStream);
            string request  = reader.ReadToEnd();

            Console.Out.WriteLine("Request:");
            Console.Out.WriteLine(request);
            
            if (context != null)
            {
                context.Logger.Log("Request:");
                context.Logger.Log(request);
            }

            AlexaResponse ar;
            
            JObject jsonRequest = JObject.Parse(request);
            string nameSpace = jsonRequest["directive"]["header"]["namespace"].Value<string>();
            switch(nameSpace) {

                case "Alexa.Authorization":
                    if (context != null)
                        context.Logger.Log("Alexa.Authorization Request");
                    ar = new AlexaResponse("Alexa.Authorization", "AcceptGrant.Response");
                    break;
                
                case "Alexa.Discovery":
                    if (context != null)
                        context.Logger.Log("Alexa.Discovery Request");
                    
                    ar = new AlexaResponse("Alexa.Discovery", "Discover.Response", "endpoint-001");

                    JObject capabilityAlexa = JObject.Parse(ar.CreatePayloadEndpointCapability());
            
                    JObject propertyPowerstate = new JObject();
                    propertyPowerstate.Add("name", "powerState");
                    JObject capabilityAlexaPowerController = JObject.Parse(ar.CreatePayloadEndpointCapability("AlexaInterface", "Alexa.PowerController", "3", propertyPowerstate.ToString()));
            
                    JArray capabilities = new JArray();
                    capabilities.Add(capabilityAlexa);
                    capabilities.Add(capabilityAlexaPowerController);
            
                    ar.AddPayloadEndpoint("endpoint-001", capabilities.ToString());
                    
                    break;

                case "Alexa.PowerController":
                    if (context != null)
                        context.Logger.Log("Alexa.PowerController Request");
                    string correlationToken = jsonRequest["directive"]["header"]["correlationToken"].Value<string>();
                    string endpointId = jsonRequest["directive"]["endpoint"]["endpointId"].Value<string>();
                    string name = jsonRequest["directive"]["header"]["name"].Value<string>();
                    
                    string state = (name == "TurnOff") ? "OFF" : "ON"; 
                    
                    bool result = SendDeviceState(endpointId, "powerState", state);
                    if (result)
                    {
                        ar = new AlexaResponse("Alexa", "Response", endpointId, "INVALID", correlationToken);
                        ar.AddContextProperty("Alexa.PowerController", "powerState", state, 200);
                    }
                    else
                    {
                        JObject payloadError = new JObject();
                        payloadError.Add("type", "ENDPOINT_UNREACHABLE");
                        payloadError.Add("message", "There wa an error setting the device state.");
                        ar = new AlexaResponse("Alexa", "ErrorResponse");
                        ar.SetPayload(payloadError.ToString());
                    }

                    break;
                
                default:
                    if (context != null)
                        context.Logger.Log("INVALID Namespace");
                    ar = new AlexaResponse();
                    break;
            }
            
            string response = ar.ToString();

            if (context != null)
            {
                context.Logger.Log("Response:");
                context.Logger.Log(response);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(response));
        }

        public bool SendDeviceState(String endpointId, String state, String value)
        {
            String attributeValue = state + "Value";
            
            AmazonDynamoDBClient client = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
            Table table = Table.LoadTable(client, "SampleSmartHome");

            Document item = new Document();
            item["ItemId"] = endpointId;
            item[attributeValue] = value;

            Task<Document> updateTask = table.UpdateItemAsync(item);
            updateTask.Wait();

            if (updateTask.Status == TaskStatus.RanToCompletion)
                return true;
            
            return false;
        }
        
    }
}