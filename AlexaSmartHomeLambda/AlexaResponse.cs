// -*- coding: utf-8 -*-

// Copyright 2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// Licensed under the Amazon Software License (the "License"). You may not use this file except in
// compliance with the License. A copy of the License is located at

//    http://aws.amazon.com/asl/

// or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlexaSmartHomeLambda
{
    public class AlexaResponse
    {
        private JObject Event = JObject.Parse("{}");
        
        private JObject Response = JObject.Parse("{}");
        private JObject Header = JObject.Parse("{}");
        private JObject Endpoint = JObject.Parse("{}");
        private JObject Payload = JObject.Parse("{}");
        
        public AlexaResponse() : this("Alexa", "Response")
        {
        }

        public AlexaResponse(string nameSpace, string name, string endpointId = "INVALID", string token = "INVALID", string correlationToken = null)
        {   
            Header.Add("namespace", CheckValue(nameSpace, "Alexa"));
            Header.Add("name", CheckValue(name, "Response"));
            Header.Add("messageId", System.Guid.NewGuid());
            Header.Add("payloadVersion", "3");

            if (correlationToken != null) {
                Header.Add("correlationToken", CheckValue(correlationToken, "INVALID"));
            }
            
            JObject scope = JObject.Parse("{}");
            scope.Add("type", "BearerToken");
            scope.Add("token", CheckValue(token, "INVALID"));

            Endpoint.Add("scope", scope);
            Endpoint.Add("endpointId", CheckValue(endpointId, "INVALID"));
            
            Event.Add("header", JToken.FromObject(Header));
            // No endpoint in an AcceptGrant or Discover request
            if (name != "AcceptGrant.Response" && name != "Discover.Response")
                Event.Add("endpoint", JToken.FromObject(Endpoint));
            Event.Add("payload", JToken.FromObject(Payload));
            
            Response.Add("event", JToken.FromObject(Event));
        }

        public void AddContextProperty(string namespaceValue = "Alexa.EndpointHealth", string name = "connectivity", string value = "{}", int uncertaintyInMilliseconds = 0)
        {
            if (Response["context"] == null)
            {
                JObject context = new JObject();
                JArray properties = new JArray();
                properties.Add(JObject.Parse(CreateContextProperty(namespaceValue, name, value, uncertaintyInMilliseconds)));
                context.Add("properties", properties);
                Response.Add("context", context);   
            }
            else
            {
                JObject context = JObject.FromObject(Response["context"]);
                JArray properties = new JArray();
                properties.Add(JObject.Parse(CreateContextProperty(namespaceValue, name, value, uncertaintyInMilliseconds)));
                context.Add("properties", properties);
                Response["context"] = context;  
            }
        }

        public string CreateContextProperty(string namespaceValue = "Alexa.EndpointHealth", string name = "connectivity", string value = "{}", int uncertaintyInMilliseconds = 0)
        {

            String valueObject;
            try
            {
                valueObject = JObject.Parse(value).ToString();
            }
            catch (JsonReaderException jre)
            {
                valueObject = value;
            }

            JObject property = new JObject();
            property.Add("namespace", namespaceValue);
            property.Add("name", name);
            property.Add("value", valueObject);
            property.Add("timeOfSample", DateTime.UtcNow);
            property.Add("uncertaintyInMilliseconds", uncertaintyInMilliseconds);

            return property.ToString();
        }
        
        public void AddCookie(string key, string value)
        {
            JObject endpoint = JObject.FromObject(Response["event"]["endpoint"]);
            JToken cookie = endpoint["cookie"];
            if (cookie != null)
            {
                endpoint["cookie"][key] = value;
            }
            else
            {
                string cookieString = string.Format("{{\"{0}\": \"{1}\"}}", key, value);
                endpoint.Add("cookie", JToken.Parse(cookieString));                
            }
            
            Response["event"]["endpoint"] = endpoint;
        }

        public void AddPayloadEndpoint(string endpointId, string capabilities)
        {
            JObject payload = JObject.FromObject(Response["event"]["payload"]);
            bool hasEndpoints = payload.TryGetValue("endpoints", out var endpointsToken);
            if (hasEndpoints)
            {
                JArray endpoints = JArray.FromObject(endpointsToken);
                endpoints.Add(JObject.Parse(CreatePayloadEndpoint(endpointId, capabilities)));
                payload["endpoints"] = endpoints;
            }
            else
            {
                JArray endpoints = new JArray();
                endpoints.Add(JObject.Parse(CreatePayloadEndpoint(endpointId, capabilities)));
                payload.Add("endpoints", endpoints);
            }
            Response["event"]["payload"] = payload;
        }

        public string CreatePayloadEndpoint(string endpointId, string capabilities, string cookie = null){
            JObject endpoint = new JObject();
            endpoint.Add("capabilities", JArray.Parse(capabilities));
            endpoint.Add("description", "Sample Endpoint Description");
            JArray displayCategories = new JArray();
            displayCategories.Add("OTHER");
            endpoint.Add("displayCategories", displayCategories);
            endpoint.Add("endpointId", endpointId);
            //endpoint.Add("endpointId", "endpoint_" + new Random().Next(0, 999999).ToString("D6"));
            endpoint.Add("friendlyName", "Sample Endpoint");
            endpoint.Add("manufacturerName", "Sample Manufacturer");

            if (cookie != null)
                endpoint.Add("cookie", JObject.Parse(cookie));

            return endpoint.ToString();
        }

        public void AddPayloadEndpointCapability(string endpointId, string capability)
        {
            JObject payload = JObject.FromObject(Response["event"]["payload"]);

            bool hasEndpoints = payload.TryGetValue("endpoints", out var endpointsToken);
            if (hasEndpoints)
            {
                JArray endpoints = JArray.FromObject(endpointsToken);
                if (endpoints.HasValues)
                {
                    foreach (JObject endpoint in endpoints.Children())
                    {
                        if (endpoint["endpointId"].ToString() == endpointId)
                        {
                            JArray.FromObject(endpoint["capabilities"]).Add(JToken.Parse(capability));
//                            Response["event"]["endpoint"] = endpoint;
                        }
                    }
                }
            }
            else
            {
                JArray endpoints = new JArray();
                payload.Add("endpoints", endpoints);
                Response["event"]["payload"] = payload;
            }
            
        }
        
        private string CheckValue(string value, string defaultValue)
        {
            if (value.Length == 0)
                return defaultValue;
            
            return value;
        }

        public string CreatePayloadEndpointCapability(string type="AlexaInterface", string interfaceValue="Alexa", string version="3", string properties=null)
        {

            JObject capability = new JObject();
            capability.Add("type", type);
            capability.Add("interface", interfaceValue);
            capability.Add("version", version);

            if (properties != null)
                capability.Add("properties", JObject.Parse(properties));             

            return capability.ToString();
        }

        public void SetPayload(string payload)
        {
            Response["event"]["payload"] = JObject.Parse(payload);
        }
        
        public override string ToString()
        {
            return Response.ToString();
        }

    }
}