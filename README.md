# RememberWhen
Sappy thing to remind wife and I of things we did in the past on an Amazon IoT button press.

## Overview
When I push this AWS IoT button, an email and text message is sent to my wife and I with a random memory from our time together.  I know, super sappy.  Building this taught me a few things and obviously made her happy.

The solution is built on .NET Core 2.1, serverless, AWS Lambda, AWS IoT, AWS SES, AWS SSM, and utilizes Twilio integration.

The following frameworks are used:
* [serverless](https://serverless.com) - creation, deployment, and invocation of AWS Lambda functions
* Moq - unit test mocking
* Xunit - unit testing framework

.NET Core's built-in dependency injection support is used as well.

There are dev and prod environments that can be deployed to and invoked by manipulating the serverless STAGE parameter.

## Design
The solution is broken down into multiple projects, each containing classes and objects with specific responsibilities.
* *RememberWhen.Lambda* - Entry point for AWS Lambda execution
* *RememberWhen.Lambda.Models* - Model classes used by various services
* *RememberWhen.Lambda.Services* - Services containing logic to retrieve a random memory, retrieve AWS application parameters, and send emails & text messages
* *RememberWhen.Lambda.Tests* - Unit tests

## Build/Deploy
Upon push of a branch, CircleCI will build and execute tests against said branch to ensure changes have not affected functionality.  Merging to master is dependent on this succeeding. 

## Future Thoughts/Ideas
* Add memories to database for persistent, non-static storage, allowing for future additions
* Add API layer to retrieve, update, and add memories to database; integrate with this API layer
* Create Alexa skill to add new memories to database via API layer
