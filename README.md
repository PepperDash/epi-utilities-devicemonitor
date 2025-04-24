![PepperDash Essentials Plugin](./images/essentials-plugin-blue.png)

# PepperDash Utilities Device Monitor

> Device Monitor Class - For use with or without Dynfusion

``` json

{
    "key" : "devMon",
    "name" : "Device Monitor",
    "group" : "api",
    "type" : "devicemonitor",
    "properties" : 
    {
        "LogToDeviceKeys": 
        [
            "DynFusion01"
        ],
        "Devices" : 
        {
            "SimplWindowsDevice1": // Key can be any string
            {
                "name" : "Test 1", // Name that will be used in log and on the bridge 
                "warningTimeout" : 60, // Timeout until warning, Required for Simpl devices only
                "errorTimeout" : 180, // Timeout until error, Required for simple devices only 
                "logToProcessor": true, // Log warnings and errors to processor, not yet implemented 
                "logToDevices": true, // Log errors to Essentials devices in the "LogToDeviceKeys" array
                "joinNumber": 1 // Bridge join number 
            },

            "EssentialsDevice1":
            {
                "name": "Display01",
                "deviceKey" : "panasonicDisplay01", // Essentials based devices require a key for the device. Devices without keys will be created as Simpl devices. 
                "logToProcessor": true,
                "logToDevices": true,
                "joinNumber": 3
            }
        }
    }
},
{
    "key": "devmon-bridge",
    "type": "eiscApiAdvanced",
    "group": "api",
    "properties": {
        "control": { "ipid": "d0", "method": "ipidTcp", "tcpSshProperties": { "address": "127.0.0.2", "port": 0 } },
        "devices": [ { "deviceKey": "devMon", "joinStart": 1 } ]
    }
}

'''
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 2.4.4
<!-- END Minimum Essentials Framework Versions -->
<!-- START Supported Types -->

<!-- END Supported Types -->
<!-- START Join Maps -->

<!-- END Join Maps -->
<!-- START Interfaces Implemented -->
### Interfaces Implemented

- ICommunicationMonitor
- IKeyed
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- JoinMapBaseAdvanced
- EssentialsBridgeableDevice
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void DeviceOnline(bool online)
- public void StopTimerSerial()
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->
### Bool Feedbacks

- IsOnlineFeedback
<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->
### Int Feedbacks

- StatusFeedback
- StatusFeedback
<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->
### String Feedbacks

- NameFeedback
- NameFeedback
<!-- END String Feedbacks -->
