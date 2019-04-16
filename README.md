Initial State .Net API Client
---

[![Build status](https://ci.appveyor.com/api/projects/status/jjm0wdu9qfxxvfgh?svg=true)](https://ci.appveyor.com/project/davidsulpy/initialstate-client-net)

Official Initial State .Net Events API Client.

# Install
This package is hosted on [nuget](https://www.nuget.org/packages/initialstate-client-net/).

`PM> Install-Package initialstate-client-net`

# Sample Use

Let's say you want to stream a telemetry measurement stored in a POCO like this:

```c#
class Telemetry {
  public int Heading { get; set; }
  public double Speed { get; set; }
  public string Status { get; set; }
}

var telemetry = new Telemetry {
  Heading = 270,
  Speed = 38.29,
  Status = "Normal"
};
```

You can stream this telemetry event simply using the `InitialStateClient`

```c#

// setup a configuration (most imporatant property is AccessKey)
var config = new InitialStateConfig {
  AccessKey = "ist_youraccesskey"
};

var bucketKey = "my_telemetry";

// instantiate an InitialStateEventsClient and inject a configuration
var initialStateEventsClient = new InitialStateEventsClient(config);

// create a bucket
initialStateEventsClient.CreateBucket(bucketKey, bucketName: "My Telemetry Bucket");

// send the events from the Telemetry POCO
initialStateEventsClient.SendEvents(telemetry, bucketKey: "my_telemetry");

```

Or perhaps you want to send a single measurement without object complexity

```c#
// send a single coordinates event
initialStateEventsClient.SendEvent(key: "location", value: "36.174465,-86.767960");

```

# License

[See LICENSE.txt](https://raw.githubusercontent.com/initialstate/initialstate-client-net/master/InitialStateClient_LICENSE.txt)
