# Deploy the Sample Code

To deploy the sample code, it must first be packaged and then uploaded to the AWS Lambda as the function code.

## Package the Sample Code

1. In your working directory, browse to the *skill-sample-csharp-smarthome-switch/AlexaSmartHomeLambda* directory.
2. In that directory and from the command-line, run the following command: `dotnet restore`
3. Then run the following command: `dotnet lambda package -c Debug -f netcoreapp2.0`
4. If successful, you should receive a message indicating "Lambda project successfully packaged" followed by a path to the zipped `AlexaSmartHomeLambda.zip` file.

If you encounter any issues, refer to the [AWS Documentation for Creating a Deployment Package (C#)](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-how-to-create-deployment-package.html).

## Upload the Sample Code

1. Navigate to https://us-east-1.console.aws.amazon.com/lambda/home?region=us-east-1#/functions/skill-sample-csharp-smarthome-switch?tab=graph and locate the *Function code* section.
2. In the _Handler_ section, replace the default handler text 'LambdaTest::LambdaTest.LambdaHandler::handleRequest' with `AlexaSmartHomeLambda::AlexaSmartHomeLambda.AlexaHandler::Handler`
3. In the _Code entry type_ dropdown, select **Upload a .ZIP file**.
4. Click the **Upload** button and browser to the `working-smarthome` directory or where you zipped the packaged code.
5. Select the `AlexaSmartHomeLambda.zip` file in the *skill-sample-csharp-smarthome-switch/AlexaSmartHomeLambda/bin/Debug/netcoreapp2.0* directory to upload as the lambda function code.
6. Click **Save** at the top right of the page.


## Test the Lambda Function
After the sample code is uploaded to the Lambda function, you can test that it is properly working via the **test events** functionality of the AWS Lambda console. This is done by creating a Test Event that contains a sample directive message that the function code should handle and respond to.

### Create a Discovery Test Event
1. On the function page for _skill-sample-csharp-smarthome-switch_, select the **Select a test event..** dropdown from the top menu of the function and click **Configure test events**.
2. In the dialog that opens, leave **Create new test event** selected and leave the default template.
3. For the _Event name_ enter: `directiveDiscovery`.
4. Copy and paste the contents of https://raw.githubusercontent.com/alexa/alexa-smarthome/master/sample_messages/Discovery/Discovery.request.json into the text field at the bottom of the dialog replacing its contents.
5. Click **Create**.

![](img/lambda-configure-test-events.png)

### Run a Discovery Test Event
1. With the **directiveDiscovery** Test Event created and selected in the dropdown, click the **Test** button.
2. If everything was successful, an "Execution result: succeeded" message will be returned and clicking on the **Details** link of the result should show a response that looks like the following:

![](img/lambda-execution-result-succeeded.png)

Any errors with the code or packaging and deployment of the code will be reflected in the result response. Not that on a successful response, the event namespace is `Alexa.Discovery`. This is because the code is responding to a test event that sends in a Discovery directive. 

Next [Test the Skill](test-the-skill.md)

Return to the [Instructions](README.md)